using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpWorld.Core.ThreadManager;
using UnityEngine;

namespace OpWorld.Render
{
  public static class StreamingMeshLoader
  {
    public class LoadRequest
    {
      public string path;
      public string modelName;
      public Action<Mesh> action;
    }

    private class LoadingRequest
    {
      public int refCount;
      public bool loaded;
      public AssetBundle bundle;
      public Mesh mesh;
    }

    private static Dictionary<string, LoadingRequest> loadingRequests = new Dictionary<string, LoadingRequest>();


    static StreamingMeshLoader()
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.playModeStateChanged -= AbortOnPlaymodeChange;
      Application.wantsToQuit -= QuitOnExit;
      UnityEditor.EditorApplication.playModeStateChanged += AbortOnPlaymodeChange;
      Application.wantsToQuit += QuitOnExit;
#endif
    }

    public static void Quit()
    {
      loadingRequests.Clear();
    }

#if UNITY_EDITOR
    static void AbortOnPlaymodeChange(UnityEditor.PlayModeStateChange state)
    {
      if (state == UnityEditor.PlayModeStateChange.ExitingEditMode || state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
        Quit();
    }
#endif

    static bool QuitOnExit()
    {
      Quit(); return true;
    }


    static public void SendLoadRequest(LoadRequest request)
    {
      if (!loadingRequests.ContainsKey(request.path))
      {
        // ��һ����������ġ�
        Load(request);
      }
      else
      {
        // ������������
        Wait(request);
      }     
    }

    static public void SendUnloadRequest(string path)
    {
      if (!loadingRequests.ContainsKey(path))
        return;

      var loading = loadingRequests[path];
      loading.refCount -= 1;
      if (loading.refCount <= 0)
      {
        if (loading.loaded)
        {
          loading.mesh = null;
          loading.bundle.Unload(true);
          loadingRequests.Remove(path);
        }
        else
        {
          var task = new CoroutineManager.Task()
          {
            routine = DoUnLoad(path, loading),
            name = "StreamingMeshLoader.Unload",
            priority = 0
          };
          task.Start();
        }
      }
    }

    private static void Load(LoadRequest request)
    {
      LoadingRequest newRequest = new LoadingRequest() { loaded = false, refCount = 1 };
      loadingRequests[request.path] = newRequest;

      var task = new CoroutineManager.Task()
      {
        routine = DoLoad(request),
        name = "StreamingMeshLoader.Load",
        priority = 0
      };
      task.Start();
    }

    private static void Wait(LoadRequest request)
    {
      var loading = loadingRequests[request.path];
      loading.refCount += 1;

      var task = new CoroutineManager.Task()
      {
        routine = DoWait(request),
        name = "StreamingMeshLoader.Wait",
        priority = 0
      };
      task.Start();
    }

    static private IEnumerator DoLoad(LoadRequest request)
    {
      //Debug.Log("loading...");

      string path = Path.Combine(Application.streamingAssetsPath, request.path);
      AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
      yield return bundleLoadRequest;
      AssetBundle myLoadedAssetBundle = bundleLoadRequest.assetBundle;
      if (myLoadedAssetBundle == null)
      {
        Debug.Log($"Failed to load AssetBundle.");
        yield break;
      }

      AssetBundleRequest assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<Mesh>(request.modelName);
      yield return assetLoadRequest;

      Mesh mesh = assetLoadRequest.asset as Mesh;
      LoadingRequest loading = loadingRequests[request.path];
      loading.loaded = true;
      loading.mesh = mesh;
      loading.bundle = myLoadedAssetBundle;
      request.action(mesh);
    }

    static private IEnumerator DoWait(LoadRequest request)
    {
      while (true)
      {
        //Debug.Log("waiting...");
        if (loadingRequests[request.path].loaded)
        {
          request.action(loadingRequests[request.path].mesh);
          break;
        }
        yield return null;
      }
    }

    private static IEnumerator DoUnLoad(string path, LoadingRequest loading)
    {
      while (true)
      {
        if (loading.refCount > 0)
        {
          break; 
        }

        if (loading.loaded)
        {
          if (loading.refCount == 0)
          {
            loading.mesh = null;
            loading.bundle.Unload(true);
            loadingRequests.Remove(path);
          }
          break;
        }
        yield return null;
      }
    }

  }
}

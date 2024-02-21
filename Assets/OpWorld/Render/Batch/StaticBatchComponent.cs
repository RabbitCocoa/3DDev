using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpWorld.Render
{
  public class StaticBatchComponent : MonoBehaviour
  {
    public StaticBatchResource subObject;
    public float hideDistance = 10f; // 在这个距离之外隐藏对象
    private MeshRenderer meshRenderer;
    bool loadedSubObject = false;
    private List<GameObject> subObjects = new List<GameObject>();

    [Serializable]
    public class StaticBatchResource
    {
      public string subObjectBundlePath;
    }


    void Start()
    {
      meshRenderer = GetComponent<MeshRenderer>();
      StartCoroutine(Check());
    }


    // Update is called once per frame
    IEnumerator Check()
    {
      while (true)
      {
        Vector3 cameraPosition = Camera.main.transform.position;
        float distanceToCamera = Vector3.Distance(transform.position, cameraPosition);

        // 根据距离决定显示或隐藏对象
        if (distanceToCamera < hideDistance)
        {
          // 在一定距离内，显示对象
          if (!loadedSubObject)
          {
            yield return Load(subObject);
            yield return Wait(subObjects);
          }
          meshRenderer.enabled = false;
        }
        else
        {
          if (loadedSubObject)
          {
            DestroySubObjects(subObjects);
          }
          meshRenderer.enabled = true;
        }

        yield return null; // 每帧检查一次
      }
    }


    IEnumerator Load(StaticBatchResource subObject)
    {
      // 异步加载AssetBundle
      string path = Path.Combine(Application.streamingAssetsPath, subObject.subObjectBundlePath);
      AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(path);
      yield return bundleLoadRequest;

      AssetBundle bundle = LoadSubObjectBundle(bundleLoadRequest);
      if (bundle != null)
      {
        LoadSubObjects(bundle);
      }
    }

    private AssetBundle LoadSubObjectBundle(AssetBundleCreateRequest bundleLoadRequest)
    {
      if (bundleLoadRequest.isDone)
      {
        AssetBundle bundle = bundleLoadRequest.assetBundle;
        if (bundle != null)
        {
          return bundle;
        }
        else
        {
          Debug.LogError("无法加载AssetBundle");
          return null;
        }
      }
      else
      {
        Debug.LogError("加载AssetBundle失败");
        return null;
      }
    }

    private void LoadSubObjects(AssetBundle bundle)
    {
      string[] assetNames = bundle.GetAllAssetNames();
      foreach (string assetName in assetNames)
      {
        GameObject prefab = bundle.LoadAsset<GameObject>(assetName);
        var go = Instantiate(prefab);
        go.SetActive(true);
        go.transform.parent = transform;
        subObjects.Add(go);
      }
      loadedSubObject = true;
      bundle.Unload(false);
    }


    private IEnumerator Wait(List<GameObject> subObjects)
    {
      // 等待所有subObject都加载好
      foreach (GameObject obj in subObjects)
      {
        StreamingMeshComponent streamMesh = obj.GetComponent<StreamingMeshComponent>();
        if (streamMesh != null && !streamMesh.IsLoaded()) {
           yield return null;
        }
      }
    }

    private void DestroySubObjects(List<GameObject> subObjects)
    {
      foreach (GameObject obj in subObjects)
      {
        Destroy(obj);
      }
      subObjects.Clear();
      loadedSubObject = false;
    }
  }
}

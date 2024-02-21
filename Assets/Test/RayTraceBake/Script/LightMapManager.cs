// /*****************************
// 项目:Test
// 文件:LightMapManager.cs
// 创建时间:13:14
// 作者:cocoa
// 描述：
// *****************************/

using System;
using System.IO;
using Sirenix.OdinInspector;
using Test.RayTraceBake.Shader;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


public enum LightmapBakingState
{
    None,
    Baking,
    Done
}

namespace Test.RayTraceBake.Script
{
    [ExecuteInEditMode]
    public class LightMapManager : MonoBehaviour
    {
        public LightmapPipelineAsset asset;
        protected MyLightmapSettings settings => asset.settings;

        public LightmapBakingState m_state = LightmapBakingState.None;

        public Light Sun;
        public RenderTexture lightmapGbufferNormal;
        public RenderTexture lightmapGbufferPosition;
        public RenderTexture lightmapRT;
        public int m_bakeFrameIndex = 0;
        public int RTGI_FrameIndex;

        public bool unwrapUV;
        public Texture2D Lightmap;

        public Texture m_currentLightmap = null;

        private static LightMapManager s_Instance;

        public static LightMapManager Instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                s_Instance = GameObject.FindObjectOfType<LightMapManager>();
                return s_Instance;
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
                return;
            if (settings.useTracing)
                RTGI_FrameIndex++;
        }

        void OnRenderObject()
        {
            if (settings.useTracing)
                RTGI_FrameIndex++;

            if (m_state == LightmapBakingState.Baking)
            {
                m_bakeFrameIndex++;
                if (m_bakeFrameIndex >= settings.targetSampleCount)
                {
                    m_state = LightmapBakingState.Done;
                }
            }
        }

        [Button("BakeLightMap")]
        void BakeLightMap()
        {
            m_state = LightmapBakingState.Baking;
            m_bakeFrameIndex = 0;
            unwrapUV = false;
            AllocateLightmapTexture();
            AllocateLightMapSpaceForObjects();
            ClearBakeData();
            var tracker = GetLightMapTracker();
            tracker.SerializeBakeRenderers(SceneManager.Instance.renderers);
        }

        void AllocateLightmapTexture()
        {
            if (lightmapGbufferNormal != null)
            {
                DestroyImmediate(lightmapGbufferNormal);
                lightmapGbufferNormal = null;
                DestroyImmediate(lightmapGbufferPosition);
                lightmapGbufferPosition = null;
            }

            lightmapGbufferNormal = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16,
                GraphicsFormat.R32G32B32A32_SFloat);
            lightmapGbufferPosition = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16,
                GraphicsFormat.R32G32B32A32_SFloat);

            if (lightmapRT != null)
            {
                DestroyImmediate(lightmapRT);
                lightmapRT = null;
            }

            lightmapRT = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16);
            lightmapRT.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
            lightmapRT.enableRandomWrite = true;
            lightmapRT.useMipMap = false;
            lightmapRT.autoGenerateMips = false;
            lightmapRT.filterMode = FilterMode.Bilinear;
            lightmapRT.Create();
            m_currentLightmap = lightmapRT;
        }

        void DeleteLightmapTexture()
        {
            if (lightmapGbufferNormal != null)
            {
                DestroyImmediate(lightmapGbufferNormal);
                lightmapGbufferNormal = null;
                DestroyImmediate(lightmapGbufferPosition);
                lightmapGbufferPosition = null;
            }

            if (lightmapRT != null)
            {
                DestroyImmediate(lightmapRT);
                lightmapRT = null;
            }
        }

        void AllocateLightMapSpaceForObjects()
        {
            //均等分uv空间
            int c = Mathf.CeilToInt(Mathf.Sqrt(SceneManager.Instance.renderers.Length));
            int tileX = 0, tileY = 0;
            float scale = 1.0f / c;


            for (int i = 0; i < SceneManager.Instance.renderers.Length; i++)
            {
                float offsetX = tileX * 1f / c;
                float offsetY = tileY * 1f / c;
                Renderer r = SceneManager.Instance.renderers[i];
                r.lightmapScaleOffset = new Vector4(scale, scale, offsetX, offsetY);

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                block.SetVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                r.SetPropertyBlock(block);

                ++tileX;
                if (tileX == c)
                {
                    tileX = 0;
                    ++tileY;
                }
            }
        }


        void ClearBakeData()
        {
            var go = GetLightMapTracker();
            go?.Clear();
        }


        LightmapTracker GetLightMapTracker()
        {
            var go = GameObject.Find("!LightMapTracker");
            if (go == null)
            {
                go = new GameObject();
                go.name = "!LightMapTracker";
                // go.hideFlags = HideFlags.HideInHierarchy;
                return go.AddComponent<LightmapTracker>();
            }

            return go.GetComponent<LightmapTracker>();
        }

        private void Start()
        {
            m_currentLightmap = null;
            CheckSceneLightMap();


            //Lightmap 状态

            // if (Lightmap == null)
            // {
            //     if (lightmapRT == null)
            //     {
            //         lightmapRT = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16);
            //         lightmapRT.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
            //         lightmapRT.enableRandomWrite = true;
            //         lightmapRT.useMipMap = false;
            //         lightmapRT.autoGenerateMips = false;
            //         lightmapRT.filterMode = FilterMode.Bilinear;
            //         lightmapRT.Create();
            //         UnityEngine.Shader.SetGlobalTexture(ShaderID._GlobalLightmap, lightmapRT);
            //     }
            //
            //     if (lightmapGbufferNormal == null)
            //     {
            //         lightmapGbufferNormal = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16,
            //             GraphicsFormat.R32G32B32A32_SFloat);
            //         lightmapGbufferPosition = new RenderTexture(settings.lightmapDim, settings.lightmapDim, 16,
            //             GraphicsFormat.R32G32B32A32_SFloat);
            //     }
            // }
            // else
            // {
            //     var tracker = GetLightMapTracker();
            //     if (tracker == null)
            //     {
            //         //报错
            //         Debug.LogError($"未保存光照数据");
            //         return;
            //     }
            //     tracker.DeserializeRenderersOnLoad();
            //     UnityEngine.Shader.SetGlobalTexture(ShaderID._GlobalLightmap, Lightmap);
            // }
            //
            // AllocateLightMapSpaceForObjects();
        }

        private void OnDestroy()
        {
            if (null != lightmapRT)
            {
                DestroyImmediate(lightmapRT);

                lightmapRT = null;
            }

            if (null != lightmapGbufferNormal)
            {
                lightmapGbufferNormal.Release();
                lightmapGbufferNormal = null;
            }

            if (null != lightmapGbufferPosition)
            {
                lightmapGbufferPosition.Release();
                lightmapGbufferPosition = null;
            }

            m_currentLightmap = null;
        }

        [Button("SaveLightMap")]
        void SaveLightMaps()
        {
            var path = GetLightMapPath();
            Lightmap = new Texture2D(settings.lightmapDim, settings.lightmapDim, TextureFormat.RGBA32, false);
            var prev = RenderTexture.active;
            RenderTexture.active = lightmapRT;
            Lightmap.ReadPixels(new Rect(0, 0, lightmapRT.width, lightmapRT.height), 0, 0);
            Lightmap.Apply();
            File.WriteAllBytes(path, Lightmap.EncodeToPNG());
            AssetDatabase.Refresh();
            RenderTexture.active = prev;
        }


        [Button("DeleteLightMap")]
        void Delete()
        {
            ClearBakeData();
            //释放Map
            DeleteLightmapTexture();
            m_bakeFrameIndex = 0;
            m_currentLightmap = null;
            //删除贴图
            m_state = LightmapBakingState.None;
            AssetDatabase.DeleteAsset(GetLightMapPath());
            AssetDatabase.Refresh();
        }

        private static string GetLightMapPath()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var directory = Path.GetDirectoryName(currentScene.path);
            var path = directory + "/" + currentScene.name + "_LM.png";
            return path;
        }

        void CheckSceneLightMap()
        {
            Lightmap = AssetDatabase.LoadAssetAtPath<Texture2D>(GetLightMapPath());
            m_state = Lightmap == null ? LightmapBakingState.None : LightmapBakingState.Done;

            if (m_state == LightmapBakingState.Done)
            {
                GetLightMapTracker().DeserializeRenderersOnLoad();
                m_currentLightmap = Lightmap;
            }
        }
    }
}
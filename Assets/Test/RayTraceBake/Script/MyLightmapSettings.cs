// /*****************************
// 项目:Test
// 文件:LightmapSettings.cs
// 创建时间:12:56
// 作者:cocoa
// 描述：
// *****************************/

using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Test.RayTraceBake.Script
{
    public enum LightmapBakeMode
    {
        Full = 0,
        Indirect = 1
    }

    [System.Serializable]
    public class MyLightmapSettings
    {
        private bool UseTracing = false;
        public bool lightonOn = true;
        [ShowInInspector]
        public bool useTracing
        {
            get => UseTracing;
            set
            {
                UseTracing = value;
                if (useTracing)
                {
                    LightMapManager.Instance.RTGI_FrameIndex = 0;
                }

                if (SceneManager.Instance != null)
                    SceneManager.Instance.isDirty = true;
            }
        }

        public LightmapBakeMode BakeModel;
        public RayTracingShader renderLightmapShader;
        public RayTracingShader RTGIShader;
        public bool m_visualizeErrorShadow = false;

        public int lightmapDim = 512; //光照贴图大小
        public Texture skylight;
        public float skyIntensity = 1;
        public int BounceCount = 10;
        public float _ShadowSoftness = 1;


        public int targetSampleCount = 1024;
    }
}
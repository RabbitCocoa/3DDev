// /*****************************
// 项目:Assembly-CSharp
// 文件:Test5_OutlineFeature.cs
// 创建时间:16:49
// 作者:cocoa
// 描述：
// *****************************/

using System;
using OpWorld.Core.Extensions.RenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Script.TestRender
{
    public class Test5_OutlineFeature : MonoBehaviour
    {
        public Camera camera;
        public Color outlineColor = Color.cyan;

        public Material renderOutlineMaskMaterial;
        private RenderTexture outlineMask;
        public LayerMask outlineMaskLayerMask;
        public bool showRT = false;

        private RenderTexture Create(float down, int aa = 1)
        {
            int rtResolutionWidth = (int)((float)camera.pixelWidth / down);
            int rtResolutionHeight = (int)((float)camera.pixelHeight / down);
            RenderTexture renderTexture = new RenderTexture(rtResolutionWidth, rtResolutionHeight, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.useMipMap = false;
            renderTexture.antiAliasing = aa;
            renderTexture.Create();
            return renderTexture;
        }

        private void Start()
        {
            outlineMask = Create(1f, 4);
        }
        void Update()
        {
            RenderObjectsMask();
        }
        
        void RenderObjectsMask()
        {
            var settings = new RenderObjectsSettings()
            {
                passName = "RenderOutlineMask",
                Event = RenderPassEvent.AfterRenderingTransparents,
                overrideMaterial = renderOutlineMaskMaterial,
                overrideMaterialPassIndex = 7,
                
            };
            settings.filterSettings.LayerMask = outlineMaskLayerMask;
            camera.RenderSceneObjects(outlineMask, settings, ClearFlag.All, Color.black);
        }
        void OnGUI()
        {
            if (GUILayout.Button("Debug mask", GUILayout.Width(150), GUILayout.Height(50)))
                showRT = !showRT;
            if (!showRT)
                return;
            GUILayout.Label(outlineMask, GUILayout.Width(320));
        }
    }
}
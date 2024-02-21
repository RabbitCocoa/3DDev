// /*****************************
// 项目:Assembly-CSharp
// 文件:Test7_Outline.cs
// 创建时间:14:00
// 作者:cocoa
// 描述：
// *****************************/

using OpWorld.Core.Extensions.RenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Script.TestRender
{
    public class Test7_Outline : MonoBehaviour
    {
        public Camera camera;

        public float blurSize = 3f;
        public float colorIntensity = 4f;
        public Color outlineColor = Color.cyan;
        
        public Material renderOutlineMaskMaterial;
        public Material renderOutlineBlurMaterial;

        public RenderTexture outlineMask;
        public RenderTexture temp0;
        public RenderTexture temp1;
        public RenderTexture temp2;
        public RenderTexture temp3;
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
        void Start()
        {
            outlineMask = Create(1f, 4);
            temp0 = Create(1f);
            temp1 = Create(2f);
            temp2 = Create(4f);
            temp3 = Create(8f);
        }
        void Update()
        {
            RenderObjectsMask();
            RenderOutline();
        }
        void RenderObjectsMask()
        {
            var settings = new RenderObjectsSettings()
            {
                passName = "RenderOutlineMask",
                Event = RenderPassEvent.AfterRenderingTransparents,
                overrideMaterial = renderOutlineMaskMaterial
            };
            settings.filterSettings.LayerMask = outlineMaskLayerMask;
            camera.RenderSceneObjects(outlineMask, settings, ClearFlag.All, Color.black);
        }
        void RenderOutline()
        {
            camera.AddPass("RenderOutline", RenderPassEvent.AfterRenderingTransparents,
                (cmd, renderingData) =>
                {
                    CameraData cameraData = renderingData.cameraData;
                    Camera camera = cameraData.camera;
                    RenderTargetIdentifier screenSource = cameraData.renderer.cameraColorTarget;

                    renderOutlineBlurMaterial.SetFloat("_BlurSize", blurSize);
                    renderOutlineBlurMaterial.SetColor("_OutlineColor", outlineColor);
                    renderOutlineBlurMaterial.SetFloat("_ColorIntensity", colorIntensity);

                    cmd.Blit(outlineMask, temp1, renderOutlineBlurMaterial, 0);
                    cmd.Blit(temp1, temp2, renderOutlineBlurMaterial, 0);
                    cmd.Blit(temp2, temp3, renderOutlineBlurMaterial, 0);
                    cmd.Blit(temp3, temp2, renderOutlineBlurMaterial, 1);
                    cmd.Blit(temp2, temp1, renderOutlineBlurMaterial, 1);
                    cmd.Blit(temp1, temp0, renderOutlineBlurMaterial, 1);

                    renderOutlineBlurMaterial.SetTexture("_MaskMap", outlineMask);
                    cmd.Blit(temp0, screenSource, renderOutlineBlurMaterial, 2);
                });
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
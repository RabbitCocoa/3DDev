// /*****************************
// 项目:Core
// 文件:CameraExtension.cs
// 创建时间:16:17
// 作者:cocoa
// 描述：
// *****************************/

using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OpWorld.Core.Extensions.RenderPipeline
{
    public static class CameraExtension
    {
        public delegate void CallBack(CommandBuffer cmd, RenderingData renderingData);

        private static MasterFeature GetMasterFeature()
        {
            try
            {
                var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
                var scriptableRendererData = (ScriptableRendererData)urpAsset.GetType()
                    .GetProperty("scriptableRendererData", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(urpAsset);
                MasterFeature feature =
                    (MasterFeature)scriptableRendererData.rendererFeatures.FirstOrDefault(x => x is MasterFeature);
                return feature;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public static void AddPass(this Camera camera, string name, RenderPassEvent evt, CallBack callback)
        {
            MasterFeature feature = GetMasterFeature();
            feature.AddPass(name, evt, new CustomPass(name, evt, callback));
        }

        public static void RenderSceneObjects(
            this Camera camera,
            RenderTexture targetTexture,
            RenderObjectsSettings settings,
            ClearFlag clearFlag,
            Color clearColor
        )
        {
            FilterSettings filter = settings.filterSettings;

            // Render Objects pass doesn't support events before rendering prepasses.
            // The camera is not setup before this point and all rendering is monoscopic.
            // Events before BeforeRenderingPrepasses should be used for input texture passes (shadow map, LUT, etc) that doesn't depend on the camera.
            // These events are filtering in the UI, but we still should prevent users from changing it from code or
            // by changing the serialized data.
            if (settings.Event < RenderPassEvent.BeforeRenderingPrePasses)
                settings.Event = RenderPassEvent.BeforeRenderingPrePasses;

            RenderObjectsPass renderObjectsPass = new RenderObjectsPass(settings.passName, settings.Event,
                filter.PassNames,
                filter.RenderQueueType, filter.LayerMask, clearColor, settings.cameraSettings);

            renderObjectsPass.Camera = camera;
            renderObjectsPass.SetRenderTarget(targetTexture);

            renderObjectsPass.OverrideMaterial = settings.overrideMaterial;
            renderObjectsPass.OverrideMaterialPassIndex = settings.overrideMaterialPassIndex;

            if (settings.overrideDepthState)
                renderObjectsPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);

            if (settings.stencilSettings.overrideStencilState)
                renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
                    settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                    settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);


            MasterFeature feature = GetMasterFeature();
            feature.AddPass(settings.passName, settings.Event, renderObjectsPass);
        }
    }
}
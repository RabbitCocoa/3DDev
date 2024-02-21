// /*****************************
// 项目:Core
// 文件:RenderObject.cs
// 创建时间:16:37
// 作者:cocoa
// 描述：
// *****************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OpWorld.Core.Extensions.RenderPipeline
{
    public enum RenderQueueType
    {
        Opaque,
        Transparent,
    }

    public class RenderObjectsSettings
    {
        public string passName = "RenderObjects";
        public RenderPassEvent Event = RenderPassEvent.AfterRendering;
        public FilterSettings filterSettings = new FilterSettings();
        public Material overrideMaterial = null;
        public int overrideMaterialPassIndex = 0;
        public bool overrideDepthState = false;
        public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
        public bool enableWrite = true;
        public StencilStateData stencilSettings = new StencilStateData();
        public CustomCameraSettings cameraSettings = new CustomCameraSettings();
    }

    public class CustomCameraSettings
    {
        public bool overrideCamera = false;
        public bool restoreCamera = true;
        public Vector4 offset;
        public float cameraFieldOfView = 60.0f;
    }

    public class FilterSettings
    {
        // TODO: expose opaque, transparent, all ranges as drop down
        public RenderQueueType RenderQueueType;
        public LayerMask LayerMask;
        public string[] PassNames;

        public FilterSettings()
        {
            RenderQueueType = RenderQueueType.Opaque;
            LayerMask = 0;
        }
    }

    public class RenderObjectsPass : AnonymousPass
    {
        private RenderQueueType m_renderQueueType;
        private RenderTexture m_targetTexture;
        private FilteringSettings m_filteringSettings;
        private CustomCameraSettings m_cameraSettings;
        private RenderStateBlock m_RenderStateBlock;
        private Color m_clearColor = Color.black;
        private ProfilingSampler m_profilingSampler;
        private List<ShaderTagId> m_shaderTagIdList = new List<ShaderTagId>();

        public Camera Camera { get; set; }
        public Material OverrideMaterial { get; set; }
        public int OverrideMaterialPassIndex { get; set; }

        public RenderTargetIdentifier ScreenSource { get; set; }

        public void SetRenderTarget(RenderTexture targetTexture)
        {
            m_targetTexture = targetTexture;
        }

        public void SetDetphState(bool writeEnabled, CompareFunction function = CompareFunction.Less)
        {
            m_RenderStateBlock.mask |= RenderStateMask.Depth;
            m_RenderStateBlock.depthState = new DepthState(writeEnabled, function);
        }

        public void SetStencilState(int reference, CompareFunction compareFunction, StencilOp passOp, StencilOp failOp,
            StencilOp zFailOp)
        {
            StencilState stencilState = StencilState.defaultValue;
            stencilState.enabled = true;
            stencilState.SetCompareFunction(compareFunction);
            stencilState.SetPassOperation(passOp);
            stencilState.SetFailOperation(failOp);
            stencilState.SetZFailOperation(zFailOp);

            m_RenderStateBlock.mask |= RenderStateMask.Stencil;
            m_RenderStateBlock.stencilReference = reference;
            m_RenderStateBlock.stencilState = stencilState;
        }


        public RenderObjectsPass(
            string profilerTag,
            RenderPassEvent renderPassEvent,
            string[] shaderTags,
            RenderQueueType renderQueueType,
            LayerMask layerMask,
            Color clearColor,
            CustomCameraSettings cameraSettings
        )
        {
            base.profilingSampler = new ProfilingSampler(nameof(RenderObjectsPass));

            this.m_clearColor = clearColor;
            m_profilingSampler = new ProfilingSampler(profilerTag);
            this.renderPassEvent = renderPassEvent;
            this.m_renderQueueType = renderQueueType;
            this.OverrideMaterial = null;
            this.OverrideMaterialPassIndex = 0;
            RenderQueueRange renderQueueRange = (renderQueueType == RenderQueueType.Transparent)
                ? RenderQueueRange.transparent
                : RenderQueueRange.opaque;
            //m_filteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            if (shaderTags != null && shaderTags.Length > 0)
            {
                foreach (var passName in shaderTags)
                    m_shaderTagIdList.Add(new ShaderTagId(passName));
            }
            else
            {
                m_shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                m_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_shaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
                m_shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            }

            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            m_cameraSettings = cameraSettings;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = CommandBufferPool.Get(m_profilingSampler.ToString());
            using (new ProfilingScope(cmd, m_profilingSampler))
            {
                cmd.SetRenderTarget(m_targetTexture);
                cmd.ClearRenderTarget(true, true, m_clearColor);
                
                Matrix4x4 projectionMatrix;
                projectionMatrix = Camera.projectionMatrix;
                //检查是否需要y轴反转
                projectionMatrix = GL.GetGPUProjectionMatrix(projectionMatrix, cameraData.IsCameraProjectionMatrixFlipped());
                
                Matrix4x4 viewMatrix = Camera.worldToCameraMatrix;
                Vector4 cameraTranslation = viewMatrix.GetColumn(3);
                viewMatrix.SetColumn(3, cameraTranslation + m_cameraSettings.offset);
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMatrix, projectionMatrix, false);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                
                SortingCriteria sortingCriteria = (m_renderQueueType == RenderQueueType.Transparent)
                    ? SortingCriteria.CommonTransparent
                    : renderingData.cameraData.defaultOpaqueSortFlags;
                
                var drawSettings = CreateDrawingSettings(m_shaderTagIdList, ref renderingData, sortingCriteria);
                drawSettings.overrideMaterial = OverrideMaterial;
                drawSettings.overrideMaterialPassIndex = OverrideMaterialPassIndex;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_filteringSettings);
                
                //重置
                RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetGPUProjectionMatrix(), false);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
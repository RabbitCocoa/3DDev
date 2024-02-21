using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Test4_Outline_Feature : ScriptableRendererFeature
{
    public Material _blurMat;
    public LayerMask _layerMask;
    [Serializable]
    public class OutlineSettings
    {
    }

    public OutlineSettings setting = new OutlineSettings();
    public static RenderTexture outlineMaskMap;


    class Test4_OutlinePass : ScriptableRenderPass
    {
        private string profilerTag;
        private ShaderTagId maskTagID;
        private ProfilingSampler _profilingSampler;
        public Material _blurMat;

        public Test4_OutlinePass(string profilerTag)
        {
            this.profilerTag = profilerTag;
            maskTagID = new ShaderTagId("WhiteMask");
            _profilingSampler = new ProfilingSampler("OutlineMaskPass");
        }

        private FilteringSettings _filteringSettings;
        private int width, height;
        private RenderTextureDescriptor _descriptor;

        public LayerMask _layerMask;

        private bool getRT;

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _filteringSettings = new FilteringSettings(RenderQueueRange.all, _layerMask.value);
            CameraData cameraData = renderingData.cameraData;
            width = cameraData.cameraTargetDescriptor.width;
            height = cameraData.cameraTargetDescriptor.height;
            _descriptor = cameraData.cameraTargetDescriptor;
            _descriptor.depthBufferBits = 0;
            if (outlineMaskMap != null)
            {
                outlineMaskMap.Release();
            }

            outlineMaskMap = new RenderTexture(_descriptor);
            RenderTargetIdentifier outlineMaskMapID = new RenderTargetIdentifier(outlineMaskMap);
            ConfigureTarget(outlineMaskMapID);
            getRT = true;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (getRT && _blurMat)
            {
                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                #region 绘制遮罩

                using (new ProfilingScope(cmd, _profilingSampler))
                {
                    cmd.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    var sortFlag = renderingData.cameraData.defaultOpaqueSortFlags;
                    //筛选包含该名字Shader的物体
                    var drawSetting = CreateDrawingSettings(maskTagID, ref renderingData, sortFlag);
                    drawSetting.overrideMaterialPassIndex = 7;
    
                    context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref _filteringSettings);
                    
                    #endregion
                }
                

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    Test4_OutlinePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new Test4_OutlinePass("Test4_OutlinePass");
        m_ScriptablePass._blurMat = _blurMat;
        m_ScriptablePass._layerMask = _layerMask;
        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
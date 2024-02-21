using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    const string bufferName = "Render Camera";

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
        litShaderTagId = new ShaderTagId("CustomLit");

    CommandBuffer buffer = new CommandBuffer { name = bufferName };
    ScriptableRenderContext context;
    Camera camera;
    CullingResults cullingResults;

    public void Render(ref ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull())
        {
            return;
        }

        Setup();
        // 不透明和透明
        DrawVisibleGeometry();
        // 粉红色的物体
        DrawUnsupportedShaders();
        // Gizmo
        DrawGizmos();
        Submit();
    }

    bool Cull()
    {
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            cullingResults = context.Cull(ref p);
            return true;
        }

        return false;
    }

    void Setup()
    {
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
        );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings){
            enableDynamicBatching = true,
            enableInstancing = true,
            perObjectData =
                PerObjectData.Lightmaps |
                PerObjectData.LightProbe |
                PerObjectData.LightProbeProxyVolume
        };
        
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
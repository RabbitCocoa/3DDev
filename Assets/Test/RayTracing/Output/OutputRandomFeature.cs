using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutputRandomFeature : RaytracingRenderFeature
{
    public OutputRandomFeature(RaytracingRenderFeatureAsset asset) : base(asset)
    {
    }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var pipelineData = _pipeline.RequirePipelineData(camera);
        var outputTarget = pipelineData.rtColor;

        var cmd = CommandBufferPool.Get(typeof(OutputRandomFeature).Name);
        using (new ProfilingSample(cmd, "RayTracing"))
        {
            cmd.SetRayTracingTextureParam(_shader,ShaderID._outputTarget,outputTarget);
            cmd.DispatchRays(_shader,"OutputColorRayGenShader",(uint)outputTarget.rt.width,(uint)outputTarget.rt.height,1,camera);
        }

        context.ExecuteCommandBuffer(cmd);
        using (new ProfilingSample(cmd, "FinalBlit"))
        {
            cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one,Vector2.zero);
        }
        context.ExecuteCommandBuffer(cmd);
    }
}

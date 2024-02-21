// /*****************************
// 项目:Test
// 文件:SimpleRayTracingFeature.cs
// 创建时间:15:56
// 作者:cocoa
// 描述：
// *****************************/

using UnityEngine;
using UnityEngine.Rendering;

namespace Test.RayTracing.RayTracing
{
    public class SoftShadowTracingFeature : RaytracingRenderFeature
    {
        SoftShadowTracingFeatureAsset _asset;
        public SoftShadowTracingFeature(RaytracingRenderFeatureAsset asset) : base(asset)
        {
            _asset = (SoftShadowTracingFeatureAsset)asset;
        }

        public override void Render(ScriptableRenderContext context, Camera camera)
        {
            base.Render(context, camera);
            var pipelineData = _pipeline.RequirePipelineData(camera);
            var outputTarget = pipelineData.rtShadow;
            var outputTargetSize = new Vector4(outputTarget.rt.width, outputTarget.rt.height, 1.0f / outputTarget.rt.width, 1.0f / outputTarget.rt.height);
            var accelerationStructure = _pipeline.RequestAccelerationStructure();
            var rng = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0, 0);

            var cmd = CommandBufferPool.Get(typeof(SoftShadowTracingFeature).Name);
            try
            {
                using (new ProfilingSample(cmd, "RayTracing"))
                {
                    cmd.SetRayTracingShaderPass(_shader, "SoftShadow");
                    cmd.SetRayTracingAccelerationStructure(_shader, ShaderID._AccelerationStructure, accelerationStructure);
                    cmd.SetRayTracingIntParam(_shader, ShaderID._FrameIndex, (int)pipelineData._frameIndex);
                    cmd.SetRayTracingFloatParam(_shader, ShaderID._ShadowSoftness, _asset.shadowSoftness);
                    cmd.SetRayTracingVectorParam(_shader, ShaderID._RNGSeed, rng);
                    cmd.SetRayTracingTextureParam(_shader, ShaderID._outputTarget, outputTarget);
                    cmd.SetRayTracingVectorParam(_shader, ShaderID._outputTargetSize, outputTargetSize);
                    cmd.DispatchRays(_shader, "SoftShadowRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
                }

                ++pipelineData._frameIndex;

                context.ExecuteCommandBuffer(cmd);

                using (new ProfilingSample(cmd, "FinalBlit"))
                {
                    cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
                }

                context.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
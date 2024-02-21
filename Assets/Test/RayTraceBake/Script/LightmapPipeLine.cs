// /*****************************
// 项目:Test
// 文件:LightmapPipeLine.cs
// 创建时间:15:25
// 作者:cocoa
// 描述：
// *****************************/

using System.Collections.Generic;
using Test.RayTraceBake.Shader;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Test.RayTraceBake.Script
{
    public class PipeLineData
    {
        private readonly Dictionary<RTHandle, RenderTextureDescriptor> _frameBuffers = new();

        //
        public RenderTextureDescriptor rtColorDescriptor =
            new RenderTextureDescriptor(1, 1, GraphicsFormat.R32G32B32A32_SFloat, 0);

        public RTHandle rtColor;
        public uint _frameIndex = 0;

        public void Add(RTHandle rt, RenderTextureDescriptor desc)
        {
            _frameBuffers.Add(rt, desc);
        }

        public void Dispose(bool disposing)
        {
            foreach (var frameBuffer in _frameBuffers)
            {
                RTHandles.Release(frameBuffer.Key);
            }

            _frameBuffers.Clear();
        }
    }

    public class LightmapPipeLine : RenderPipeline
    {
        private readonly Dictionary<int, PipeLineData> _pipeLineDatas = new();
        private RayTracingAccelerationStructure _accelerationStructure;
        private string k_customLightmapOn = "CUSTOM_LIGHTMAP_ON";
        private LightmapPipelineAsset _asset;

        private CameraRenderer m_render = new CameraRenderer();
        private LightmapRenderer m_lightmapRenderer;
        public LightmapPipeLine(LightmapPipelineAsset asset)
        {
            _asset = asset;
            _accelerationStructure = new RayTracingAccelerationStructure();
            m_lightmapRenderer = new LightmapRenderer(asset);
        }


        private void RenderLightmapUvGbuffer(ref ScriptableRenderContext cmd)
        {
            if (LightMapManager.Instance.m_state == LightmapBakingState.Baking)
            {
                if (LightMapManager.Instance.unwrapUV)
                    return;
                LightMapManager.Instance.unwrapUV = true;
                var cmd1 = CommandBufferPool.Get("UnwrapUV");

                m_lightmapRenderer.RenderLightmapUvGbuffer(cmd1, LightMapManager.Instance.lightmapGbufferNormal,
                    LightMapManager.Instance.lightmapGbufferPosition);
                cmd1.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                cmd.ExecuteCommandBuffer(cmd1);
                cmd1.Clear();
                cmd.Submit();
                CommandBufferPool.Release(cmd1);
            }
        }

        private RTHandle AllocateRT(string name, Camera camera, RenderTextureDescriptor desc)
        {
            return RTHandles.Alloc(
                camera.pixelWidth / desc.width,
                camera.pixelHeight / desc.height,
                1,
                DepthBits.None,
                desc.graphicsFormat,
                FilterMode.Point,
                TextureWrapMode.Clamp,
                TextureDimension.Tex2D,
                true,
                false,
                false,
                false,
                1,
                0,
                MSAASamples.None,
                false,
                false,
                RenderTextureMemoryless.None, name
            );
        }

        private PipeLineData RequirePipelineData(Camera camera)
        {
            var id = camera.GetInstanceID();
            if (_pipeLineDatas.TryGetValue(id, out var pipelineData))
                return pipelineData;
            var data = new PipeLineData();
            _pipeLineDatas.Add(id, data);

            data.rtColor = AllocateRT($"rtColor{camera.name}", camera, data.rtColorDescriptor);
            data.Add(data.rtColor, data.rtColorDescriptor);

            return data;
        }

        private void BakeLightmap(ref ScriptableRenderContext context)
        {
            if (LightMapManager.Instance.m_state == LightmapBakingState.Baking)
            {
                var cmd = CommandBufferPool.Get("RenderLightmapUvGbuffer");

                m_lightmapRenderer.BakeLightmap(this, cmd, LightMapManager.Instance.lightmapRT,
                    LightMapManager.Instance.lightmapGbufferNormal,
                    LightMapManager.Instance.lightmapGbufferPosition);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }

        private void RenderRayTracing(ref ScriptableRenderContext context, Camera camera)
        {
            PipeLineData pipeLineData = RequirePipelineData(camera);
            RTHandle outputHandle = pipeLineData.rtColor;
            var cmd = CommandBufferPool.Get("RTGI");
            m_lightmapRenderer.RenderTracing(this, cmd, outputHandle, camera,
                LightMapManager.Instance.lightmapGbufferNormal, LightMapManager.Instance.lightmapGbufferPosition);
            context.ExecuteCommandBuffer(cmd);
            // 2 blit to screen
            using (new ProfilingSample(cmd, "FinalBlit"))
            {
                cmd.Blit(outputHandle, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            if (!SystemInfo.supportsRayTracing)
            {
                Debug.LogError(
                    "You system is not support ray tracing. Please check your graphic API is D3D12 and os is Windows 10.");
                return;
            }

            BeginFrameRendering(context, cameras);

            System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
            RenderLightmapUvGbuffer(ref context);

            BuildAccelerationStructure();
            if (cameras.Length > 0 && cameras[0].cameraType == CameraType.SceneView && !_asset.settings.useTracing)
                BakeLightmap(ref context);
    
            if (_asset != null && _asset.settings.lightonOn) {
                UnityEngine.Shader.EnableKeyword(k_customLightmapOn);
            } else {
                UnityEngine.Shader.DisableKeyword(k_customLightmapOn);
            }

            foreach (var camera in cameras)
            {
                // Only render game and scene view camera.
                if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
                    continue;

                BeginCameraRendering(context, camera);
                SetupCamera(camera);
                if (_asset.settings.useTracing)
                {
                    RenderRayTracing(ref context, camera);
                }
                else
                {
                    UnityEngine.Shader.SetGlobalTexture(ShaderID._GlobalLightmap,
                        LightMapManager.Instance.m_currentLightmap);

                    m_render.Render(ref context, camera);
                }

                context.Submit();
                EndCameraRendering(context, camera);
            }

            EndFrameRendering(context, cameras);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (_accelerationStructure != null)
            {
                _accelerationStructure.Dispose();
                _accelerationStructure = null;
            }

            if (_pipeLineDatas != null)
            {
                foreach (var pipeLineData in _pipeLineDatas.Values)
                {
                    pipeLineData.Dispose(disposing);
                }

                _pipeLineDatas.Clear();
            }
        }

        private static void SetupCamera(Camera camera)
        {
            UnityEngine.Shader.SetGlobalVector(ShaderID._WorldSpaceCameraPos, camera.transform.position);
            var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false); //透视矩阵
            var viewMatrix = camera.worldToCameraMatrix; //视图矩阵
            var viewProjMatrix = projMatrix * viewMatrix; // 视图 透视 矩阵
            var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix); // 透视-视图 逆矩阵 将uv变到世界坐标的矩阵
            UnityEngine.Shader.SetGlobalMatrix(ShaderID._InvCameraViewProj, invViewProjMatrix);
            UnityEngine.Shader.SetGlobalFloat(ShaderID._CameraFarDistance, camera.farClipPlane);
        }

        public RayTracingAccelerationStructure RequestAccleration()
        {
            return _accelerationStructure;
        }

        private void BuildAccelerationStructure()
        {
            if (SceneManager.Instance == null || !SceneManager.Instance.isDirty) return;

            _accelerationStructure.Dispose();
            _accelerationStructure = new RayTracingAccelerationStructure();

            SceneManager.Instance.FillAccelerationStructure(ref _accelerationStructure);

            _accelerationStructure.Build();

            SceneManager.Instance.isDirty = false;
        }
    }
}
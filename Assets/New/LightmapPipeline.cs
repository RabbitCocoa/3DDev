using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;


public class PipelineData
{
  public RenderTextureDescriptor rtColorDescriptor = new RenderTextureDescriptor(1, 1, GraphicsFormat.R32G32B32A32_SFloat, 0);
  public RTHandle rtColor;
  public uint _frameIndex = 0;
  private readonly Dictionary<RTHandle, RenderTextureDescriptor> _framebuffers = new Dictionary<RTHandle, RenderTextureDescriptor>();
  public void Add(RTHandle handle, RenderTextureDescriptor desc)
  {
    _framebuffers.Add(handle, desc);
  }

  public void Dispose(bool disposing)
  {
    foreach (var pair in _framebuffers) {
      RTHandles.Release(pair.Key);
    }
    _framebuffers.Clear();
  }
}


public class LightmapPipeline : RenderPipeline
{
  private LightmapPipelineAsset _asset;

  private RayTracingAccelerationStructure m_accelerationStructure;

  private readonly Dictionary<int /*cameraId*/, PipelineData> _pipelineData = new Dictionary<int, PipelineData>();

  private CameraRenderer m_renderer = new CameraRenderer();

  public LightmapPipeline(LightmapPipelineAsset asset)
  {
    _asset = asset;
    m_accelerationStructure = new RayTracingAccelerationStructure();
  }

  private void BuildAccelerationStructure()
  {
    if (SceneManager.Instance == null || !SceneManager.Instance.isDirty) return;

    m_accelerationStructure.Dispose();
    m_accelerationStructure = new RayTracingAccelerationStructure();

    SceneManager.Instance.FillAccelerationStructure(ref m_accelerationStructure);

    m_accelerationStructure.Build();

    SceneManager.Instance.isDirty = false;
  }
  

  public RayTracingAccelerationStructure RequestAccelerationStructure()
  {
    return m_accelerationStructure;
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
      0f,
      MSAASamples.None,
      false,
      false,
      RenderTextureMemoryless.None,
      name);
  }
  public PipelineData RequirePipelineData(Camera camera)
  {
    var id = camera.GetInstanceID();

    if (_pipelineData.TryGetValue(id, out var pipelineData))
      return pipelineData;

    PipelineData data = new PipelineData();
    _pipelineData.Add(id, data);

    data.rtColor = AllocateRT($"rtColor{camera.name}", camera, data.rtColorDescriptor);
    data.Add(data.rtColor, data.rtColorDescriptor);

    return data;
  }

  private void RenderLightmapUvGbuffer(ref ScriptableRenderContext context)
  {
    var cmd = CommandBufferPool.Get("RenderLightmapUvGbuffer");
    LightmapRenderer.Instance.RenderLightmapUvGbuffer(cmd);
    //context.ExecuteCommandBuffer(cmd1);
    cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
    context.ExecuteCommandBuffer(cmd);
    cmd.Clear();
    context.Submit();
    CommandBufferPool.Release(cmd);
  }
  private void BakeLightmap(ref ScriptableRenderContext context)
  {
    var cmd = CommandBufferPool.Get("BakeLightmap");
    LightmapRenderer.Instance.BakeLightmap(this, cmd);
    context.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
  }

  private void RenderRTGI(ref ScriptableRenderContext context, Camera camera)
  {
    // 1 render raytracing
    PipelineData pipelineData = RequirePipelineData(camera);
    RTHandle outputTarget = pipelineData.rtColor;
    var cmd = CommandBufferPool.Get("RTGI");
    LightmapRenderer.Instance.RenderRTGI(this, cmd, outputTarget, camera);
    context.ExecuteCommandBuffer(cmd);

    // 2 blit to screen
    using (new ProfilingSample(cmd, "FinalBlit")) {
      cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
    }
    context.ExecuteCommandBuffer(cmd);
    CommandBufferPool.Release(cmd);
  }

  private static void SetupCamera(Camera camera)
  {
    Shader.SetGlobalVector(ShaderID._WorldSpaceCameraPos, camera.transform.position);
    var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
    var viewMatrix = camera.worldToCameraMatrix;
    var viewProjMatrix = projMatrix * viewMatrix;
    var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
    Shader.SetGlobalMatrix(ShaderID._InvCameraViewProj, invViewProjMatrix);
    Shader.SetGlobalFloat(ShaderID._CameraFarDistance, camera.farClipPlane);
  }


  protected override void Render(ScriptableRenderContext context, Camera[] cameras)
  {
    if (!SystemInfo.supportsRayTracing) {
      Debug.LogError("You system is not support ray tracing. Please check your graphic API is D3D12 and os is Windows 10.");
      return;
    }
    BeginFrameRendering(context, cameras);

    System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));

    RenderLightmapUvGbuffer(ref context);

    BuildAccelerationStructure();

    if (cameras.Length > 0 && cameras[0].cameraType == CameraType.Game && !LightmapRenderer.Instance.m_lightmapSetting.useRaytracing) {
      BakeLightmap(ref context);
    }

    // render scene
    foreach (var camera in cameras) {
      if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
        continue;

      BeginCameraRendering(context, camera);
      SetupCamera(camera);

      if (LightmapRenderer.Instance.m_lightmapSetting.useRaytracing) {
        RenderRTGI(ref context, camera);
      }
      else {
        m_renderer.Render(context, camera);
      }
      context.Submit();
      EndCameraRendering(context, camera);
    }

    EndFrameRendering(context, cameras);
  }

  protected override void Dispose(bool disposing)
  {
    if (null != m_accelerationStructure) {
      m_accelerationStructure.Dispose();
      m_accelerationStructure = null;
    }

    if (null != _pipelineData) {
      foreach (var data in _pipelineData) {
        data.Value.Dispose(disposing);
      }
      _pipelineData.Clear();
    }
  }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class PipelineData
{
  public RenderTextureDescriptor rtColorDescriptor = new RenderTextureDescriptor(1, 1, GraphicsFormat.R32G32B32A32_SFloat, 0);
  public RTHandle rtColor;
  
  public RenderTextureDescriptor rtShadowDescriptor = new RenderTextureDescriptor(1, 1, GraphicsFormat.R32G32B32A32_SFloat, 0);
  public RTHandle rtShadow;
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


public class RayTracingRenderPipeline : RenderPipeline
{
  private RayTracingRenderPipelineAsset _asset;
  private RayTracingAccelerationStructure _accelerationStructure; //加速结构
  private readonly Dictionary<int, PipelineData> _pipelineData = new Dictionary<int, PipelineData>();
  private RaytracingRenderFeature _feature1;

  public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset)
  {
    _asset = asset;
    _accelerationStructure = new RayTracingAccelerationStructure();
    _feature1 = _asset.tutorialAsset.CreateTutorial();
    if (_feature1 == null)
    {
      Debug.LogError("Can't create tutorial.");
      return;
    }

    if (_feature1.Init(this) == false)
    {
      _feature1 = null;
      Debug.LogError("Initialize tutorial failed.");
      return;
    }
  }

  public RayTracingAccelerationStructure RequestAccelerationStructure()
  {
    return _accelerationStructure;
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

    data.rtShadow = AllocateRT($"rtshadow{camera.name}", camera, data.rtShadowDescriptor);
    data.Add(data.rtShadow, data.rtShadowDescriptor);
    
    data.rtColor = AllocateRT($"rtcolor{camera.name}", camera, data.rtColorDescriptor);
    data.Add(data.rtColor, data.rtColorDescriptor);

    return data;
  }
  
  protected override void Render(ScriptableRenderContext context, Camera[] cameras)
  {
    if (!SystemInfo.supportsRayTracing)
    {
      Debug.LogError("You system is not support ray tracing. Please check your graphic API is D3D12 and os is Windows 10.");
      return;
    }

    BeginFrameRendering(context, cameras);

    System.Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));

    BuildAccelerationStructure();

    foreach (var camera in cameras)
    {
      // Only render game and scene view camera.
      if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
        continue;

      BeginCameraRendering(context, camera);
      _feature1?.Render(context, camera);
      context.Submit();
      EndCameraRendering(context, camera);
    }

    EndFrameRendering(context, cameras);
  }

  protected override void Dispose(bool disposing)
  {
    if (null != _feature1)
    {
      _feature1.Dispose(disposing);
      _feature1 = null;
    }

    if (null != _pipelineData) 
    {
      foreach (var data in _pipelineData) {
        data.Value.Dispose(disposing);
      }
      _pipelineData.Clear();
    }

    if (null != _accelerationStructure)
    {
      _accelerationStructure.Dispose();
      _accelerationStructure = null;
    }
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

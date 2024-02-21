using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class LightmapRenderer : MonoBehaviour
{
  public LightmapSettings m_lightmapSetting;
  public RayTracingShader m_renderLightmapShader;
  public RayTracingShader m_RTGIShader;
  public bool m_visualizeErrorShadow = false;

  public Camera m_lightmapCamera;
  public Light m_sun;


  // gbuffers
  public RenderTexture m_lightmapGbufferNormal = null;
  public RenderTexture m_lightmapGbufferPosition= null;

  // lightmap data
  private string k_customLightmapOn = "CUSTOM_LIGHTMAP_ON";
  public RenderTexture m_lightmapRT = null;
  public Texture2D m_lightmap = null;
  public uint m_bakeFrameIndex = 0;
  public uint m_rtgiFrameIndex = 0;


  private static LightmapRenderer s_Instance;
  public static LightmapRenderer Instance
  {
    get
    {
      if (s_Instance != null) return s_Instance;
      s_Instance = GameObject.FindObjectOfType<LightmapRenderer>();
      return s_Instance;
    }
  }

  void Start()
  {
    // 分配 lightmap uv 空间
    // 均匀 c x c 等分
    int c = Mathf.CeilToInt(Mathf.Sqrt(SceneManager.Instance.renderers.Length));
    int tileX = 0, tileY = 0;
    float scale = 1.0f / c;
    for(int i = 0; i < SceneManager.Instance.renderers.Length; ++i)
    {
      float offsetX = (float)tileX / (float)c;
      float offsetY = (float)tileY / (float)c;
      Renderer r = SceneManager.Instance.renderers[i];
      var scaleOffset = new Vector4(scale, scale, offsetX, offsetY);
      r.lightmapScaleOffset = scaleOffset;
      MaterialPropertyBlock block = new MaterialPropertyBlock();
      r.gameObject.GetComponent<Renderer>().GetPropertyBlock(block);
      block.SetVector(ShaderID._ObjectLightmapUvST, scaleOffset);
      r.gameObject.GetComponent<Renderer>().SetPropertyBlock(block);
      ++tileX;
      if (tileX == c) {
        tileX = 0;
        ++tileY;
      }
    }
  }
  private void Update()
  {
    if (m_lightmapSetting.useRaytracing) {
      if (m_rtgiFrameIndex < m_lightmapSetting.targetSampleCount) {
        ++m_rtgiFrameIndex;
      }
    }
    else {
      if (m_bakeFrameIndex < m_lightmapSetting.targetSampleCount) {
        ++m_bakeFrameIndex;
      }
    }
  }

  void OnDestroy()
  {

    if (null != m_lightmapRT) {
      m_lightmapRT = null;
      Destroy(m_lightmapRT);
    }

    Shader.DisableKeyword(k_customLightmapOn);
  }
  
  public void DrawAllMeshes(CommandBuffer cmd, int pass)
  {
    for (int i = 0; i < SceneManager.Instance.renderers.Length; ++i) {
      Renderer r = SceneManager.Instance.renderers[i];
      var mesh = r.gameObject.GetComponent<MeshFilter>();
      var renderer = r.gameObject.GetComponent<MeshRenderer>();
      cmd.SetGlobalVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
      cmd.DrawMesh(mesh.mesh, r.gameObject.transform.localToWorldMatrix, renderer.material, 0, pass);
    }
  }

  internal void UnwrapUV2(CommandBuffer cmd, RenderTexture rt, int gbufferType, int pass)
  {
    cmd.SetRenderTarget(rt);
    cmd.ClearRenderTarget(true, true, Color.black);
    // set pass global shader data
    cmd.SetGlobalInt(ShaderID._LightmapUnwrapMode, gbufferType);
    // render meshes with jitter
    // 保守光栅化
    float lightmapDimRcpX = 1.0f / m_lightmapSetting.lightmapDim;
    float lightmapDimRcpY = 1.0f / m_lightmapSetting.lightmapDim;
    float jitterSize = 2.0f;
    for (int i = 0; i < 64; ++i) {
      var halton = HaltonSequence.GetHaltonSequence(i);
      Vector4 lightmapUnwrapJitter = new Vector4
      {
        x = (halton.x * 2 - 1) * lightmapDimRcpX * jitterSize,
        y = (halton.y * 2 - 1) * lightmapDimRcpY * jitterSize
      };
      cmd.SetGlobalVector(ShaderID._LightmapUnwrapJitter, lightmapUnwrapJitter);
      // render all meshes
      DrawAllMeshes(cmd, pass);
    }
    cmd.SetGlobalVector(ShaderID._LightmapUnwrapJitter, new Vector4(0, 0, 0, 0));
    DrawAllMeshes(cmd, pass);
  }

  int cc = 0;
  public void RenderLightmapUvGbuffer(CommandBuffer cmd)
  {
    if (m_lightmapGbufferNormal == null) {
      m_lightmapGbufferNormal = new RenderTexture(m_lightmapSetting.lightmapDim, m_lightmapSetting.lightmapDim, 16, GraphicsFormat.R16G16B16A16_SFloat);
      m_lightmapGbufferPosition = new RenderTexture(m_lightmapSetting.lightmapDim, m_lightmapSetting.lightmapDim, 16, GraphicsFormat.R16G16B16A16_SFloat);
    }

    if (cc == 0) {
      cc = 1;
      const int metaPass = 1;
      UnwrapUV2(cmd, m_lightmapGbufferNormal, 0, metaPass);
      UnwrapUV2(cmd, m_lightmapGbufferPosition, 1, metaPass);
    }
  }

  public void BakeLightmap(LightmapPipeline lightmapPipeline, CommandBuffer cmd)
  {
    if (m_bakeFrameIndex >= m_lightmapSetting.targetSampleCount) {
      return;
    }

    if (m_lightmapRT == null) {
      //m_lightmap = new Texture2D(m_lightmapDim, m_lightmapDim, TextureFormat.RGBA32, false);
      m_lightmapRT = new RenderTexture(m_lightmapSetting.lightmapDim, m_lightmapSetting.lightmapDim, 16);
      m_lightmapRT.graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat;
      m_lightmapRT.enableRandomWrite = true;
      m_lightmapRT.useMipMap = false;
      m_lightmapRT.autoGenerateMips = false;
      m_lightmapRT.filterMode = FilterMode.Bilinear;
      m_lightmapRT.Create();

      Shader.SetGlobalTexture("_GlobalLightmap", m_lightmapRT);
    }

    var outputTarget = m_lightmapRT;
    var outputTargetSize = new Vector4(outputTarget.width, outputTarget.height, 1.0f / outputTarget.width, 1.0f / outputTarget.height);
    var accelerationStructure = lightmapPipeline.RequestAccelerationStructure();
    var rng = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0, 0);

    cmd.SetRayTracingShaderPass(m_renderLightmapShader, "RenderLightmap");
    cmd.SetRayTracingAccelerationStructure(m_renderLightmapShader, ShaderID._AccelerationStructure, accelerationStructure);
    var lightDir = m_sun ? m_sun.transform.forward : new Vector3(-1, -1, -1).normalized;
    cmd.SetRayTracingVectorParam(m_renderLightmapShader, ShaderID._SunDir, lightDir);
    float sunIntensity = m_sun ? m_sun.intensity : 0;
    cmd.SetRayTracingFloatParam(m_renderLightmapShader, ShaderID._SunIntensity, sunIntensity);
    cmd.SetRayTracingTextureParam(m_renderLightmapShader, ShaderID._TexSkyLight, m_lightmapSetting.skylight);
    cmd.SetRayTracingFloatParam(m_renderLightmapShader, ShaderID._SkyLightIntensity, m_lightmapSetting.skylightIntensity);
    cmd.SetRayTracingIntParam(m_renderLightmapShader, ShaderID._BounceCount, m_lightmapSetting.bounceCount);
    cmd.SetRayTracingFloatParam(m_renderLightmapShader, ShaderID._ShadowSoftness, m_lightmapSetting.shadowSoftness);
    cmd.SetRayTracingIntParam(m_renderLightmapShader, ShaderID._FrameIndex, (int)m_bakeFrameIndex);
    cmd.SetRayTracingVectorParam(m_renderLightmapShader, ShaderID._RNGSeed, rng);
    cmd.SetRayTracingTextureParam(m_renderLightmapShader, ShaderID._outputTarget, outputTarget);
    cmd.SetRayTracingTextureParam(m_renderLightmapShader, ShaderID._LightmapGbufferPosition, m_lightmapGbufferPosition);
    cmd.SetRayTracingTextureParam(m_renderLightmapShader, ShaderID._LightmapGbufferNormal, m_lightmapGbufferNormal);
    cmd.SetRayTracingVectorParam(m_renderLightmapShader, ShaderID._outputTargetSize, outputTargetSize);
    cmd.SetRayTracingIntParam(m_renderLightmapShader, ShaderID._VisualizeErrorShadow, m_visualizeErrorShadow ? 1 : 0);
    cmd.DispatchRays(m_renderLightmapShader, "RenderLightmapRayGenShader", (uint)outputTarget.width, (uint)outputTarget.height, 1);

    Shader.EnableKeyword(k_customLightmapOn);
  }

  public void RenderRTGI(LightmapPipeline lightmapPipeline, CommandBuffer cmd, RTHandle outputTarget, Camera camera)
  {
    var outputTargetSize = new Vector4(outputTarget.rt.width, outputTarget.rt.height, 1.0f / outputTarget.rt.width, 1.0f / outputTarget.rt.height);
    var accelerationStructure = lightmapPipeline.RequestAccelerationStructure();
    var rng = new Vector4(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0, 0);
    cmd.SetRayTracingShaderPass(m_RTGIShader, "RenderLightmap");
    cmd.SetRayTracingAccelerationStructure(m_RTGIShader, ShaderID._AccelerationStructure, accelerationStructure);
    var lightDir = m_sun ? m_sun.transform.forward : new Vector3(-1, -1, -1).normalized;
    cmd.SetRayTracingVectorParam(m_RTGIShader, ShaderID._SunDir, lightDir);
    float sunIntensity = m_sun ? m_sun.intensity : 0;
    cmd.SetRayTracingFloatParam(m_RTGIShader, ShaderID._SunIntensity, sunIntensity);
    cmd.SetRayTracingTextureParam(m_RTGIShader, ShaderID._TexSkyLight, m_lightmapSetting.skylight);
    cmd.SetRayTracingFloatParam(m_RTGIShader, ShaderID._SkyLightIntensity, m_lightmapSetting.skylightIntensity);
    cmd.SetRayTracingIntParam(m_RTGIShader, ShaderID._BounceCount, (int)m_lightmapSetting.bounceCount);
    cmd.SetRayTracingFloatParam(m_RTGIShader, ShaderID._ShadowSoftness, m_lightmapSetting.shadowSoftness);
    cmd.SetRayTracingIntParam(m_RTGIShader, ShaderID._FrameIndex, (int)m_rtgiFrameIndex);
    cmd.SetRayTracingVectorParam(m_RTGIShader, ShaderID._RNGSeed, rng);
    cmd.SetRayTracingTextureParam(m_RTGIShader, ShaderID._outputTarget, outputTarget);
    cmd.SetRayTracingVectorParam(m_RTGIShader, ShaderID._outputTargetSize, outputTargetSize);
    cmd.DispatchRays(m_RTGIShader, "RTGIRayGenShader", (uint)outputTarget.rt.width, (uint)outputTarget.rt.height, 1, camera);

  }
}

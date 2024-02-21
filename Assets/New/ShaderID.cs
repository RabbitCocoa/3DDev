using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class ShaderID
{
  public static readonly int _WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
  public static readonly int _InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");
  public static readonly int _CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");
  public static readonly int _outputTarget = Shader.PropertyToID("_OutputTarget");
  public static readonly int _outputTargetSize = Shader.PropertyToID("_OutputTargetSize");
  public static readonly int _GbufferNormalDepth = Shader.PropertyToID("_GbufferNormalDepth");
  public static readonly int _AccelerationStructure = Shader.PropertyToID("_AccelerationStructure");
  public static readonly int _FrameIndex = Shader.PropertyToID("_FrameIndex");
  public static readonly int _RNGSeed = Shader.PropertyToID("_RNGSeed");
  public static readonly int _ShadowSoftness = Shader.PropertyToID("_ShadowSoftness");
  
  // lightmap
  public static readonly int _BounceCount = Shader.PropertyToID("_BounceCount");
  public static readonly int _SunDir = Shader.PropertyToID("_SunDir");
  public static readonly int _SunIntensity = Shader.PropertyToID("_SunIntensity");
  public static readonly int _TexSkyLight = Shader.PropertyToID("_TexSkyLight");
  public static readonly int _SkyLightIntensity = Shader.PropertyToID("_SkyLightIntensity");
  public static readonly int _LightmapGbufferPosition = Shader.PropertyToID("_LightmapGbufferPosition");
  public static readonly int _LightmapGbufferNormal = Shader.PropertyToID("_LightmapGbufferNormal");
  public static readonly int _LightmapUnwrapMode = Shader.PropertyToID("_LightmapUnwrapMode");
  public static readonly int _LightmapUnwrapJitter = Shader.PropertyToID("_LightmapUnwrapJitter");
  public static readonly int _ObjectLightmapUvST = Shader.PropertyToID("_ObjectLightmapUvST");
  public static readonly int _VisualizeErrorShadow = Shader.PropertyToID("_VisualizeErrorShadow");



}


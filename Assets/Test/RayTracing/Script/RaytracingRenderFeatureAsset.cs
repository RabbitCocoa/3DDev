using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class RaytracingRenderFeatureAsset : ScriptableObject
{
  public RayTracingShader shader;


  public abstract RaytracingRenderFeature CreateTutorial();
}

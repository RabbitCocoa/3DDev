using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightmapSettings", menuName = "Rendering/LightmapSettings", order = -1)]
public class LightmapSettings : ScriptableObject
{
  public bool useRaytracing = false;
  public int bounceCount = 4;
  public int lightmapDim = 512;
  [Range(1, 100)] public float shadowSoftness = 10;
  public int targetSampleCount = 1024;
  public Texture skylight;
  public float skylightIntensity = 1.0f;
}

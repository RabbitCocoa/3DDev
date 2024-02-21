using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "LightmapPipelineAsset", menuName = "Rendering/LightmapPipelineAsset", order = -1)]
public class LightmapPipelineAsset : RenderPipelineAsset
{
  protected override RenderPipeline CreatePipeline()
  {
    return new LightmapPipeline(this);
  }
}

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

/// <summary>
/// the ray tracing render pipeline asset.
/// </summary>
[CreateAssetMenu(fileName = "RayTracingRenderPipelineAsset", menuName = "Rendering/RayTracingRenderPipelineAsset",
    order = -1)]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
    /// <summary>
    /// the tutorial asset.
    /// </summary>
    public RaytracingRenderFeatureAsset tutorialAsset;

    /// <summary>
    /// create the render pipeline.
    /// </summary>
    /// <returns>the render pipeline.</returns>
    protected override RenderPipeline CreatePipeline()
    {
        return new RayTracingRenderPipeline(this);
    }
}
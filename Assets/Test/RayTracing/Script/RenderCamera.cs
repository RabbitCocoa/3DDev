using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderCamera : MonoBehaviour
{
  public RenderPipelineAsset renderPipelineAsset;
  private RenderPipelineAsset _oldRenderPipelineAsset;

  public IEnumerator Start()
  {
    yield return new WaitForEndOfFrame();
    _oldRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
    GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
    QualitySettings.renderPipeline = renderPipelineAsset;
  }

  [Button]
  public void Change()
  {
    GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
    QualitySettings.renderPipeline = renderPipelineAsset;
  }

  public void OnDestroy()
  {
    GraphicsSettings.renderPipelineAsset = _oldRenderPipelineAsset;
    QualitySettings.renderPipeline = _oldRenderPipelineAsset;
  }
}

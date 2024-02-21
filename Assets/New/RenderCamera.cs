using System.Collections;
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


  public void OnDestroy()
  {
    GraphicsSettings.renderPipelineAsset = _oldRenderPipelineAsset;
    QualitySettings.renderPipeline = _oldRenderPipelineAsset;
  }
}

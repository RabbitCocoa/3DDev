// /*****************************
// 项目:Test
// 文件:LightmapPipelineAsset.cs
// 创建时间:12:54
// 作者:cocoa
// 描述：
// *****************************/

using Test.RayTraceBake.Script;
using UnityEngine;
using UnityEngine.Rendering;

namespace Test.RayTraceBake.Shader
{
    [CreateAssetMenu(fileName = "LightmapPipelineAsset", menuName = "Rendering/LightmapPipelineAsset",
        order = -1)]
    public class LightmapPipelineAsset : RenderPipelineAsset
    {
        public MyLightmapSettings settings;
        protected override RenderPipeline CreatePipeline()
        {
            return new LightmapPipeLine(this);
        }
    }
}
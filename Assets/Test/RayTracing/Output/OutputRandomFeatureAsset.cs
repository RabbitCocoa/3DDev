// /*****************************
// 项目:Test
// 文件:OutputRandomFeatureAsset.cs
// 创建时间:18:37
// 作者:cocoa
// 描述：
// *****************************/

using UnityEngine;

namespace Test.RayTracing
{
    [CreateAssetMenu(fileName = "OutputRandomFeatureAsset", menuName = "Rendering/OutputRandomFeatureAsset",
        order = -1)]
    public class OutputRandomFeatureAsset: RaytracingRenderFeatureAsset
    {
        public override RaytracingRenderFeature CreateTutorial()
        {
            return new OutputRandomFeature(this);
        }
    }
}
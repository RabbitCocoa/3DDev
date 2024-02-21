// /*****************************
// 项目:Test
// 文件:RayTracingFeatureAsset.cs
// 创建时间:15:56
// 作者:cocoa
// 描述：
// *****************************/

using UnityEngine;

namespace Test.RayTracing.RayTracing
{
    [CreateAssetMenu(fileName = "SimpleRayTracingFeatureAsset", menuName = "Rendering/SimpleRayTracingFeatureAsset",
        order = -1)]
    public class SimpleRayTracingFeatureAsset : RaytracingRenderFeatureAsset
    {
        public override RaytracingRenderFeature CreateTutorial()
        {
            return new SimpleRayTracingFeature(this);
        }
    }
}
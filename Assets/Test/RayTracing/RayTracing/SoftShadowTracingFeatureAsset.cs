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
    [CreateAssetMenu(fileName = "SoftShadowTracingFeatureAsset", menuName = "Rendering/SoftShadowTracingFeatureAsset",
        order = -1)]
    public class SoftShadowTracingFeatureAsset : RaytracingRenderFeatureAsset
    {
        public float shadowSoftness;

        public override RaytracingRenderFeature CreateTutorial()
        {
          return  new SoftShadowTracingFeature(this);
        }
    }
}
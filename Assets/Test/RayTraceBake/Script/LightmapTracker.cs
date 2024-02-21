// /*****************************
// 项目:Test
// 文件:LightmapTracker.cs
// 创建时间:16:40
// 作者:cocoa
// 描述：
// *****************************/

using System.Collections.Generic;
using UnityEngine;

namespace Test.RayTraceBake.Script
{
    public class LightmapTracker : MonoBehaviour
    {
        public List<Renderer> allRenderers;
        public List<Vector4> allScaleOffset;


        public void Clear()
        {
            allRenderers.Clear();
            allScaleOffset.Clear();
        }

        public void SerializeBakeRenderers(Renderer[] renderers)
        {
            allRenderers.Clear();
            allScaleOffset.Clear();
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                allRenderers.Add(renderer);
                allScaleOffset.Add(renderer.lightmapScaleOffset);
            }
        }

        public void DeserializeRenderersOnLoad()
        {
            for (int i = 0; i < allRenderers.Count; i++)
            {
                Renderer r = allRenderers[i];
                r.lightmapScaleOffset = allScaleOffset[i];

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                block.SetVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                r.SetPropertyBlock(block);
            }
        }
    }
}
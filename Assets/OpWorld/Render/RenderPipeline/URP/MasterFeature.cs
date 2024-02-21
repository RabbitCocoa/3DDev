// /*****************************
// 项目:Core
// 文件:MasterFeature.cs
// 创建时间:16:20
// 作者:cocoa
// 描述：
// *****************************/

using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace OpWorld.Core.Extensions.RenderPipeline
{
    public class MasterFeature : ScriptableRendererFeature
    {
        
        private Dictionary<string, AnonymousPass> m_currentPasses;
        
        public override void Create()
        {
            m_currentPasses = new Dictionary<string, AnonymousPass>();
        }
        public void AddPass(string name, RenderPassEvent evt, AnonymousPass pass)
        {
            if (m_currentPasses.TryGetValue(name, out var gg))
            {
                return;
            }
            m_currentPasses.Add(name, pass);
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (var p in m_currentPasses)
            {
                renderer.EnqueuePass(p.Value);
            }
            m_currentPasses.Clear();
        }
    }
}
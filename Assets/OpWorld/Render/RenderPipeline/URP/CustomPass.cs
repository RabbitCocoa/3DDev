// /*****************************
// 项目:Core
// 文件:CustomPass.cs
// 创建时间:16:33
// 作者:cocoa
// 描述：
// *****************************/

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OpWorld.Core.Extensions.RenderPipeline
{
    public class CustomPass : AnonymousPass
    {
        private string passName;
        private CameraExtension.CallBack callback;

        public CustomPass(string passName, RenderPassEvent evt, CameraExtension.CallBack callback)
        {
            this.passName = passName;
            this.callback = callback;
            renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(passName);
            cmd.Clear();
            callback(cmd, renderingData);
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}
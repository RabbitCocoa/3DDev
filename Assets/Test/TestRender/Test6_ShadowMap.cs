// /*****************************
// 项目:Assembly-CSharp
// 文件:Test6_ShadowMap.cs
// 创建时间:13:28
// 作者:cocoa
// 描述：
// *****************************/

using OpWorld.Core.Extensions.RenderPipeline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Script.TestRender
{
    public class ShadowmapHelper
    {

        static public RenderTexture CreateShadowmap(int shadowMapDim)
        {
            RenderTexture renderTexture = new RenderTexture(shadowMapDim, shadowMapDim, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.useMipMap = false;
            renderTexture.autoGenerateMips = false;
            renderTexture.Create();
            return renderTexture;
        }
    }
    
    public class Test6_ShadowMap : MonoBehaviour
    {
        [SerializeField] int dim = 1024;
        [SerializeField] Light light;
        public Camera camera;
        public Material material;
        public RenderTexture shadowmap;
        public LayerMask mask;
        void Start()
        {
            shadowmap = ShadowmapHelper.CreateShadowmap(dim);
        }
        
        [ContextMenu("render shadowmap")]
        void Offline()
        {
            shadowmap = ShadowmapHelper.CreateShadowmap(dim);
            Shader.SetGlobalFloat("_ShadowMapMinDepth", 0);
            Shader.SetGlobalFloat("_ShadowMapDepthRange", 64);
            var setting = new RenderObjectsSettings()
            {
                passName = "Test RenderObjs",
                Event = RenderPassEvent.AfterRendering,
                overrideMaterial = material
            };
            setting.filterSettings.LayerMask = mask;
            camera.RenderSceneObjects(shadowmap, setting, ClearFlag.All, Color.white);
        }

        
    }
}
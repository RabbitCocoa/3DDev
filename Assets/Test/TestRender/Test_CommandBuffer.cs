using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Test_CommandBuffer : ScriptableRendererFeature
{
    public struct SphereInfo
    {
        public float4x4 transform;
        public float3 color;
    };
    
    public Material material;
    public Mesh mesh;
    public static int sphereCount = 20;

    #region computerBuffer

  
    private static SphereInfo[] sphereInfos;
    private static ComputeBuffer computeBuffer;

    public static ComputeBuffer ComputeBuffer
    {
        get
        {
            if (computeBuffer == null)
            {
                computeBuffer = new ComputeBuffer(sphereCount,Marshal.SizeOf(typeof(SphereInfo)));
                sphereInfos = new SphereInfo[sphereCount];
                SetData();
            }else if (computeBuffer.count != sphereCount)
            {
                computeBuffer.Release();
                computeBuffer = new ComputeBuffer(sphereCount,Marshal.SizeOf(typeof(SphereInfo)));
                sphereInfos = new SphereInfo[sphereCount];
                SetData();
            }

            void SetData(){
                for (int i = 0; i < sphereInfos.Length; i++)
                {
                    sphereInfos[i].transform =
                        Matrix4x4.TRS(new Vector3(1f * i, 0, 0), Quaternion.identity, Vector3.one);
                    sphereInfos[i].color = new float3(i * 1.0f / 255f, 0, 0);
                }
                computeBuffer.SetData(sphereInfos);

            }

            return computeBuffer;
        }
    }

    private static MaterialPropertyBlock _materialPropertyBlock;

    public static MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            if (_materialPropertyBlock == null)
            {
                _materialPropertyBlock = new MaterialPropertyBlock();
            }

            _materialPropertyBlock.SetBuffer(ShaderProperties._SphereInfo,ComputeBuffer);
            return _materialPropertyBlock;
        }
    }

    private class ShaderProperties
    {
        public static readonly int _SphereInfo = Shader.PropertyToID("_SphereInfo");
    }

    #endregion

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material material;
        public Mesh mesh;
        public int sphereCount;

        public CustomRenderPass(Material material, Mesh mesh, int sphereCount)
        {
            this.material = material;
            this.mesh = mesh;
            this.sphereCount = sphereCount;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DrawSphere");
            cmd.DrawMeshInstancedProcedural(mesh, 0,material, 0, sphereCount,
                MaterialPropertyBlock);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(material, mesh, sphereCount);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material != null && mesh != null && sphereCount > 0)
            renderer.EnqueuePass(m_ScriptablePass);
    }
}
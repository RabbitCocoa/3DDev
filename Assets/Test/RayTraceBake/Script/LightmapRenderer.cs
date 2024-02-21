using System;
using System.Collections;
using System.Collections.Generic;
using Test.RayTraceBake.Script;
using Test.RayTraceBake.Shader;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class LightmapRenderer
{
    public LightmapRenderer(LightmapPipelineAsset asset)
    {
        this.asset = asset;
    }

    public LightmapPipelineAsset asset;
    protected MyLightmapSettings settings => asset.settings;






    private string k_customLightmapOn = "CUSTOM_LIGHTMAP_ON";


    void OnDestroy()
    {
        Shader.DisableKeyword(k_customLightmapOn);
    }

    public void RenderTracing(LightmapPipeLine pipeLine, CommandBuffer cmd, RTHandle rtHandle, Camera camera,
        RenderTexture lightmapGbufferNormal,
        RenderTexture lightmapGbufferPosition)
    {
        var outputTarget = rtHandle.rt;
        var outputTargetSize = new Vector4(outputTarget.width, outputTarget.height, 1f / outputTarget.width,
            1f / outputTarget.height);
        var acclerationStructure = pipeLine.RequestAccleration();

        var rng = new Vector4(Random.Range(0f, 1f), Random.Range(0f, 1f), 0, 0);

        RayTracingShader RTGIShader = settings.RTGIShader;

        cmd.SetRayTracingShaderPass(RTGIShader, "RenderLightmap");
        cmd.SetRayTracingAccelerationStructure(RTGIShader, ShaderID._AccelerationStructure,
            acclerationStructure);
        Light Sun = LightMapManager.Instance.Sun;
        var lightDir =  Sun ? Sun.transform.forward : Vector3.down;
        float sunIntensity = Sun?.intensity ?? 0;
        //天空盒
        cmd.SetRayTracingTextureParam(RTGIShader, ShaderID._TexSkyLight, settings.skylight);
        cmd.SetRayTracingFloatParam(RTGIShader, ShaderID._SkyLightIntensity, settings.skyIntensity);
        //反弹次数
        cmd.SetRayTracingIntParam(RTGIShader, ShaderID.BounceCount, settings.BounceCount);
        //阴影
        cmd.SetRayTracingFloatParam(RTGIShader, ShaderID._ShadowSoftness, settings._ShadowSoftness);
        //光照
        cmd.SetRayTracingVectorParam(RTGIShader, ShaderID._SunDir, lightDir);
        cmd.SetRayTracingFloatParam(RTGIShader, ShaderID._SunIntensity, sunIntensity);
        //随机数和帧数
        cmd.SetRayTracingVectorParam(RTGIShader, ShaderID._RNGSeed, rng);
        cmd.SetRayTracingIntParam(RTGIShader, ShaderID._FrameIndex, LightMapManager.Instance.RTGI_FrameIndex);
        //烘培贴图
     //   cmd.SetRayTracingTextureParam(RTGIShader, ShaderID._LightmapGbufferNormal, lightmapGbufferNormal);
     //   cmd.SetRayTracingTextureParam(RTGIShader, ShaderID._LightmapGbufferPosition, lightmapGbufferPosition);
        //OutputTarget
        cmd.SetRayTracingTextureParam(RTGIShader, ShaderID._outputTarget, outputTarget);
        cmd.SetRayTracingVectorParam(RTGIShader, ShaderID._outputTargetSize, outputTargetSize);
        cmd.DispatchRays(RTGIShader, "RTGIRayGenShader", (uint)outputTarget.width,
            (uint)outputTarget.height, 1, camera);
        //BakeMode
        cmd.SetRayTracingIntParam(RTGIShader,ShaderID._LightmapBakeMode,(int)LightmapBakeMode.Full);
        Shader.EnableKeyword(k_customLightmapOn);
    }


    void UnwrapUV(CommandBuffer cmd, RenderTexture texture, int gbufferType)
    {
        //  SetupCamera(texture);


        cmd.SetRenderTarget(texture);
        cmd.ClearRenderTarget(true, true, Color.black);
        //set pass global shader data
        cmd.SetGlobalInt(ShaderID._LightmapUnwrapMode, gbufferType);

        float lightmapDimRcpx = 1.0f / settings.lightmapDim;
        float lightmapDimRcpy = 1.0f / settings.lightmapDim;
        float jitterSize = 2f; //偏移尺寸大小

        for (int i = 0; i < 8; i++)
        {
            var halton = HaltonSequence.GetHaltonSequence(i);
            Vector4 lightmapUnwarJitter = new Vector4()
            {
                x = (halton.x * 2 - 1) * jitterSize * lightmapDimRcpx,
                y = (halton.y * 2 - 1) * jitterSize * lightmapDimRcpy,
            };

            cmd.SetGlobalVector(ShaderID._LightmapUnwrapJitter, lightmapUnwarJitter);
            //render mesh
            DrawAllMesheWithMetaPass(cmd);
        }

        cmd.SetGlobalVector(ShaderID._LightmapUnwrapJitter, new Vector4(0, 0, 0, 0));
        DrawAllMesheWithMetaPass(cmd);
    }


    public void RenderLightmapUvGbuffer(CommandBuffer cmd, RenderTexture lightmapGbufferNormal,
        RenderTexture lightmapGbufferPosition)
    {
        UnwrapUV(cmd, lightmapGbufferNormal, 0);
        UnwrapUV(cmd, lightmapGbufferPosition, 1);
    }

    internal void DrawAllMesheWithMetaPass(CommandBuffer cmd)
    {
        for (int i = 0; i < SceneManager.Instance.renderers.Length; i++)
        {
            Renderer r = SceneManager.Instance.renderers[i];
            int pass = r.sharedMaterial.FindPass("Meta");
            if (r is MeshRenderer)
            {
                var mesh = r.gameObject.GetComponent<MeshFilter>();
                var renderer = r.gameObject.GetComponent<MeshRenderer>();
                //set object shader data
                // MaterialPropertyBlock block = new MaterialPropertyBlock();
                // r.GetPropertyBlock(block);
                // block.SetVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                // r.SetPropertyBlock(block);

                cmd.SetGlobalVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                //draw 
                cmd.DrawMesh(mesh.sharedMesh, mesh.transform.localToWorldMatrix, renderer.sharedMaterial, 0, pass);
            }
            else if (r is SkinnedMeshRenderer render)
            {
                var mesh = render.sharedMesh;
                //set object shader data
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                r.GetPropertyBlock(block);
                block.SetVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                r.SetPropertyBlock(block);

                cmd.SetGlobalVector(ShaderID._ObjectLightmapUvST, r.lightmapScaleOffset);
                //draw 
                for (int j = 0; j < render.sharedMaterials.Length; j++)
                {
                    cmd.DrawMesh(mesh, render.transform.parent.localToWorldMatrix, render.sharedMaterials[i], i, pass);
                }
            }
        }
    }

    public void BakeLightmap(LightmapPipeLine pipeline, CommandBuffer cmd, RenderTexture lightmapRT,
        RenderTexture lightmapGbufferNormal,
        RenderTexture lightmapGbufferPosition)
    {
        var outputTarget = lightmapRT;
        var outputTargetSize = new Vector4(outputTarget.width, outputTarget.height, 1f / outputTarget.width,
            1f / outputTarget.height);
        //  BuildAcclerationStructure();
        var acclerationStructure = pipeline.RequestAccleration();
        var rng = new Vector4(Random.Range(0f, 1f), Random.Range(0f, 1f), 0, 0);

        RayTracingShader renderLightmapShader = settings.renderLightmapShader;

        cmd.SetRayTracingShaderPass(renderLightmapShader, "RenderLightmap");
        cmd.SetRayTracingAccelerationStructure(renderLightmapShader, ShaderID._AccelerationStructure,
            acclerationStructure);
        Light Sun = LightMapManager.Instance.Sun;
        var lightDir = Sun ? Sun.transform.forward : Vector3.down;
        float sunIntensity = Sun?.intensity ?? 0;
        //天空盒
        cmd.SetRayTracingTextureParam(renderLightmapShader, ShaderID._TexSkyLight, settings.skylight);
        cmd.SetRayTracingFloatParam(renderLightmapShader, ShaderID._SkyLightIntensity, settings.skyIntensity);
        //反弹次数
        cmd.SetRayTracingIntParam(renderLightmapShader, ShaderID.BounceCount, settings.BounceCount);
        //阴影
        cmd.SetRayTracingFloatParam(renderLightmapShader, ShaderID._ShadowSoftness, settings._ShadowSoftness);
        //光照
        cmd.SetRayTracingVectorParam(renderLightmapShader, ShaderID._SunDir, lightDir);
        cmd.SetRayTracingFloatParam(renderLightmapShader, ShaderID._SunIntensity, sunIntensity);
        //随机数和帧数
        cmd.SetRayTracingVectorParam(renderLightmapShader, ShaderID._RNGSeed, rng);
        cmd.SetRayTracingIntParam(renderLightmapShader, ShaderID._FrameIndex, LightMapManager.Instance.m_bakeFrameIndex);
        //烘培贴图
        cmd.SetRayTracingTextureParam(renderLightmapShader, ShaderID._LightmapGbufferNormal, lightmapGbufferNormal);
        cmd.SetRayTracingTextureParam(renderLightmapShader, ShaderID._LightmapGbufferPosition, lightmapGbufferPosition);
        //OutputTarget
        cmd.SetRayTracingTextureParam(renderLightmapShader, ShaderID._outputTarget, outputTarget);
        cmd.SetRayTracingVectorParam(renderLightmapShader, ShaderID._outputTargetSize, outputTargetSize);

        //可视化错误阴影
        cmd.SetRayTracingIntParam(renderLightmapShader, ShaderID._VisualizeErrorShadow,
            settings.m_visualizeErrorShadow ? 1 : 0);

        //模式
        cmd.SetRayTracingIntParam(renderLightmapShader,ShaderID._LightmapBakeMode,(int)settings.BakeModel);
        
        cmd.DispatchRays(renderLightmapShader, "RenderLightmapRayGenShader", (uint)outputTarget.width,
            (uint)outputTarget.height, 1);

        Shader.EnableKeyword(k_customLightmapOn);
    }
}
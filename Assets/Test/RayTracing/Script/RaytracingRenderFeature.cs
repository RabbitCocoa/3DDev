using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public abstract class RaytracingRenderFeature
{
    protected RayTracingRenderPipeline _pipeline;
    protected RaytracingRenderFeatureAsset _asset;
    protected RayTracingShader _shader;

    protected RaytracingRenderFeature(RaytracingRenderFeatureAsset asset)
    {
        _asset = asset;
    }

    public virtual bool Init(RayTracingRenderPipeline pipeline)
    {
        _pipeline = pipeline;
        _shader = _asset.shader;

        return true;
    }

    public virtual void Render(ScriptableRenderContext context, Camera camera)
    {
        SetupCamera(camera);
    }

    public virtual void Dispose(bool disposing)
    {
    }

    private static void SetupCamera(Camera camera)
    {
        Shader.SetGlobalVector(ShaderID._WorldSpaceCameraPos, camera.transform.position);
        var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false); //透视矩阵
        var viewMatrix = camera.worldToCameraMatrix; //视图矩阵
        var viewProjMatrix = projMatrix * viewMatrix; // 视图 透视 矩阵
        var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix); // 透视-视图 逆矩阵 将uv变到世界坐标的矩阵
        Shader.SetGlobalMatrix(ShaderID._InvCameraViewProj, invViewProjMatrix);
        Shader.SetGlobalFloat(ShaderID._CameraFarDistance, camera.farClipPlane);
    }
}
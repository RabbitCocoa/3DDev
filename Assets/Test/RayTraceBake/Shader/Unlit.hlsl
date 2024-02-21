#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Unlit.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include"GlobalILumination.hlsl"


CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseColor;
CBUFFER_END


float2 TransformBaseUV(float2 baseUV)
{
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    return baseUV * baseST.xy + baseST.zw;
}

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    GI_ATTRIBUTE_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    float2 uv :TEXCOORD0;
    float4 vertex : SV_POSITION;
    GI_VARYINGS_DATA
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


v2f vert(appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    TRANSFER_GI_DATA(v, o);

    // o.uvLm = v.uv1 * _ObjectLightmapUvST.xy + _ObjectLightmapUvST.zw;
    // o.uvLm.y = 1 - o.uvLm.y;
    o.uv =
        TransformBaseUV(v.uv);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    // sample the texture

    //采样周围的像素 取平均值


    GI gi = GetGI(GI_FRAGMENT_DATA(i));

    half3 mainTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
    // apply fog
    //  UNITY_APPLY_FOG(i.fogCoord, col);
    return half4(gi.diffuse * mainTex, 1);
}

#endif

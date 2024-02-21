#ifndef LITMETAPASS_DEFINE
#define LITMETAPASS_DEFINE
#include "UnityCG.cginc"
#include "UnityStandardInput.cginc"

#define LIGHTMAP_UNWRAP_MODE_NORMAL 0
#define LIGHTMAP_UNWRAP_MODE_POSITION 1

struct v2f
{
    float4 vertex : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float3 wPos : TEXCOORD1;
};


int _LightmapUnwrapMode;
float4 _ObjectLightmapUvST;
float2 _LightmapUnwrapJitter;


v2f MetaPassVert(VertexInput v)
{
    v2f o;
    //用uv当坐标
    //将uv从(0,1)变换到 -1，1 这是齐次坐标空间系
    float2 uv1 = v.uv1;
    uv1 = uv1 * _ObjectLightmapUvST.xy + _ObjectLightmapUvST.zw;
    o.vertex = float4(uv1 * 2 - 1, 0, 1);
    //加上空间偏移
    o.vertex.xy += _LightmapUnwrapJitter.xy;
    o.Normal = UnityObjectToWorldNormal(v.normal);
    o.wPos = mul(unity_ObjectToWorld, v.vertex).xyz;

    return o;
}

fixed4 MetaPassFrag(v2f i) : SV_Target
{
    fixed4 col = 0;
    if (_LightmapUnwrapMode == LIGHTMAP_UNWRAP_MODE_NORMAL)
    {
        float3 dxdyWspos = max(abs(ddx(i.wPos)), abs(ddy(i.wPos)));
        float dwspos = max(max(dxdyWspos.x, dxdyWspos.y), dxdyWspos.z) * sqrt(2);
        col = float4(normalize(i.Normal) * 0.5 + 0.5, dwspos);
    }
    else
    {
        col = float4(i.wPos, 1);
    }
    return col;
}


#endif

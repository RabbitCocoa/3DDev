#ifndef CUSTOM_GI_INCLUDE
#define CUSTOM_GI_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#if 1 //自己的LightMap
#define _UNITY_LIGHTMAP  _GlobalLightmap
#define _SAMPLER_UNITY_LIGHTMAP  sampler_GlobalLightmap
float4 _ObjectLightmapUvST;
#define _UNITY_LIGHTMAPST _ObjectLightmapUvST
#define _LIGHTMAP_ON CUSTOM_LIGHTMAP_ON
#else //Unity的LightMap
#define _UNITY_LIGHTMAP  unity_Lightmap
#define _SAMPLER_UNITY_LIGHTMAP  samplerunity_Lightmap
#define _UNITY_LIGHTMAPST unity_LightmapST
#define _LIGHTMAP_ON LIGHTMAP_ON
#endif

TEXTURE2D(_UNITY_LIGHTMAP);
SAMPLER(_SAMPLER_UNITY_LIGHTMAP);


#if  _LIGHTMAP_ON
#define GI_ATTRIBUTE_DATA float2 lightMapUV : TEXCOORD1;
#define GI_VARYINGS_DATA float2 lightMapUV : TEXCOORD1;
#define TRANSFER_GI_DATA(input, output) \
output.lightMapUV = input.lightMapUV * _UNITY_LIGHTMAPST.xy + _UNITY_LIGHTMAPST.zw; \
output.lightMapUV.y = 1 - output.lightMapUV.y;
#define GI_FRAGMENT_DATA(input) input.lightMapUV
#else
#define GI_ATTRIBUTE_DATA
#define GI_VARYINGS_DATA
#define TRANSFER_GI_DATA(input, output)
#define GI_FRAGMENT_DATA(input) 0.0
#endif


real3 CustomSampleSingleLightmap(
    TEXTURE2D_LIGHTMAP_PARAM(lightmapTex, lightmapSampler), LIGHTMAP_EXTRA_ARGS, bool encodedLightmap,
    real4 decodeInstructions)
{
    real3 illuminance = real3(0.0, 0.0, 0.0);
    if (encodedLightmap)
    {
        //real4 encodedIlluminance = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapTex, lightmapSampler, LIGHTMAP_EXTRA_ARGS_USE).rgba;
        //illuminance = DecodeLightmap(encodedIlluminance, decodeInstructions);
    }
    else
    {
        illuminance = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapTex, lightmapSampler, LIGHTMAP_EXTRA_ARGS_USE).rgb;
    }
    return illuminance;
}

float3 SampleLightMap(float2 lightMapUV)
{
    #if  _LIGHTMAP_ON
    return CustomSampleSingleLightmap(
        TEXTURE2D_ARGS(_UNITY_LIGHTMAP, _SAMPLER_UNITY_LIGHTMAP), lightMapUV,
        #if 1
        false,
        #else
                true,
        #endif
        float4(LIGHTMAP_HDR_MULTIPLIER, LIGHTMAP_HDR_EXPONENT, 0.0, 0.0)
    );
    #else
    return 0;
    #endif
}


struct GI
{
    float3 diffuse;
};


GI GetGI(float2 lightMapUV)
{
    GI gi;
    gi.diffuse = SampleLightMap(lightMapUV);
    return gi;
}


//struct GI

//Texture lightmap
//sampler

//SampleLightmap

#endif

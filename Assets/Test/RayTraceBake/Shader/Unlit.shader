Shader "Shadowmap/UnLit"
{
    Properties
    {
        _BaseColor("BaseColor",Color) = (1,1,1,1)
        _BaseMap("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "CustomLit"
            }
            HLSLPROGRAM
                   #pragma shader_feature _CLIPPING
            #pragma shader_feature _RECEIVE_SHADOWS
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
            #pragma multi_compile _ _CASCADE_BLEND_SOFT _CASCADE_BLEND_DITHER
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ CUSTOM_LIGHTMAP_ON
            #pragma multi_compile_instancing
            #include"Unlit.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            ENDHLSL
        }


        Pass
        {
            Name "META"
            Tags
            {
                "LightMode" = "Meta"
            }
            Cull Off
            CGPROGRAM
            #include "LitMetaPass.hlsl"
            #pragma vertex MetaPassVert
            #pragma  fragment MetaPassFrag
            ENDCG
        }
    }



    SubShader
    {
        Pass
        {
            Name "RenderLightmap"
            Tags
            {
                "LightMode" = "RayTracing"
            }

            HLSLPROGRAM
            #pragma raytracing test
            #include"LitRayTracingMetaPass.hlsl"
            ENDHLSL
        }
    }
}
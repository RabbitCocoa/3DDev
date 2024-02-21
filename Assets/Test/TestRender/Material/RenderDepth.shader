Shader "Custom/RenderDepth"
{
    Properties
    {
        _Cull("__cull", Float) = 2.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Cull[_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            float _ShadowMapMinDepth;
            float _ShadowMapDepthRange;
            #define CAMERA_FAR  _ShadowMapDepthRange

            
            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float depthVS:TEXCOORD1;
            };
            

         

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.vertex);
                float3 positionWS = TransformObjectToWorld(v.vertex);
                float3 positionVS = TransformWorldToView(positionWS);
                o.depthVS = -positionVS.z;
                return o;
            }


            half4 frag(v2f i) : SV_Target
            {
                float d = max(0,i.depthVS - _ShadowMapMinDepth);
                return float4(d/ CAMERA_FAR,d/CAMERA_FAR,0,1);
            }
            ENDHLSL
        }
    }
}
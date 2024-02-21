Shader "Custom/DrawMesh"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID :SV_InstanceID;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;

                float3 color: TEXCOORD1;
            };

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct SphereInfo
            {
                float4x4 transform;
                float3 color;
            };
            StructuredBuffer<SphereInfo> _SphereInfo;
            void setup(){}
            
            #endif
            
            v2f vert(appdata v)
            {
                v2f o;
                o.positionWS = TransformObjectToWorld (v.vertex);

                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                uint instanceID = v.instanceID;
                SphereInfo s = _SphereInfo[instanceID];
                o.positionWS = mul(s.transform,v.vertex).xyz;
                o.positionWS.y += sin(_Time.y+instanceID);
                o.color = s.color;

                #endif
                o.positionCS = TransformWorldToHClip(o.positionWS);

                return o;
            }


            half4 frag(v2f i) : SV_Target
            {
                return half4(i.color, 1);
            }
            ENDHLSL
        }
    }
}
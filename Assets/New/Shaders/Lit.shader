Shader "Unlit/VisualizeObjectLightmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityStandardInput.cginc"
            #include "UnityStandardCore.cginc"

            struct v2f
            {
                float2 uvLM : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            sampler2D _GlobalLightmap;
            float4 _ObjectLightmapUvST;

            v2f vert (VertexInput v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvLM = v.uv1; // [0..1]
                o.uvLM = o.uvLM * _ObjectLightmapUvST.xy + _ObjectLightmapUvST.zw;
                o.uvLM.y = 1 - o.uvLM.y;
                //o.vertex = float4(o.uvLM * 2 - 1, 1, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
              //fixed4 col = tex2D(_MainTex, i.uv);
              fixed4 col = tex2D(_GlobalLightmap, i.uvLM);
              return col;
            }
            ENDCG
        }
    
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityStandardInput.cginc"

            struct v2f
            {
                float3 normal : TEXCOORD0;
                float3 wspos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            #define LIGHTMAP_UNWRAP_MODE_NORMAL 0
            #define LIGHTMAP_UNWRAP_MODE_POSITION 1

            int _LightmapUnwrapMode;
            float2 _LightmapUnwrapJitter;
            float4 _ObjectLightmapUvST;

            v2f vert(VertexInput v)
            {
              v2f o;
              float2 uv1 = v.uv1;
              uv1 = uv1 * _ObjectLightmapUvST.xy + _ObjectLightmapUvST.zw;
              o.vertex = float4(uv1 * 2 - 1, 0, 1);
              o.vertex.xy += _LightmapUnwrapJitter.xy;
              //o.uv = v.uv1;
              o.wspos = mul(unity_ObjectToWorld, v.vertex).xyz;
              o.normal = UnityObjectToWorldNormal(v.normal);
              return o;
            }

            float4 frag(v2f i) : SV_Target
            {
              float4 col = 0;
              if (_LightmapUnwrapMode == LIGHTMAP_UNWRAP_MODE_NORMAL)
              {
                float3 dxdyWspos = max(abs(ddx(i.wspos)), abs(ddy(i.wspos)));
                float dwspos = max(max(dxdyWspos.x, dxdyWspos.y), dxdyWspos.z) * sqrt(2);
                col = float4(normalize(i.normal) * 0.5 + 0.5, dwspos);
              }
              else
              {
                 col = float4(i.wspos, 1);
              }
              //float4 col = float4(i.uv, 0, 1);
              return col;
            }
            ENDCG
        }
    }
   
    SubShader
    {
        Pass
        {
            Name "RenderLightmap"
            Tags { "LightMode" = "RayTracing" }

            HLSLPROGRAM

            #pragma raytracing test
            #include "./Common.hlsl"
            #include "./RaytracingIntersection.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            CBUFFER_END
            
             [shader("closesthit")]
            void MetaPassCloestHit(inout MetaRayPayload rayPayload :SV_RayPayload,
                               AttributeData attributeData:SV_IntersectionAttributes)
            {
                float3 origin = WorldRayOrigin();
                float3 rayDirection = WorldRayDirection();
                float t = RayTCurrent();
                float3 wspos = origin + rayDirection * t;

                if (rayPayload.rayType != RayTypeShadow)
                {
                    IntersectionVertex vertex;
                    GetCurrentIntersectionVertex(attributeData, vertex);
                    float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
                    rayPayload.hit = true;
                    rayPayload.surface.normal = normalize(mul(objectToWorld, vertex.normalOS));
                    rayPayload.surface.ffnormal = dot(rayPayload.surface.normal, rayDirection) <= 0.0
                                                     ? rayPayload.surface.normal
                                                     : rayPayload.surface.normal * -1.0;
                    rayPayload.surface.position = wspos;
         
                    float3 UpVector = abs(rayPayload.surface.ffnormal.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
                    rayPayload.surface.tangent = normalize(cross(UpVector, rayPayload.surface.ffnormal));
                    rayPayload.surface.bitangent = -normalize(
                        cross(rayPayload.surface.ffnormal, rayPayload.surface.tangent));
                    rayPayload.surface.material.albedo = _Color.rgb ;
                }
                else
                {
                    IntersectionVertex vertex;
                    GetCurrentIntersectionVertex(attributeData, vertex);
                    float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
                    rayPayload.surface.normal = normalize(mul(objectToWorld, vertex.normalOS));
                    rayPayload.hit = true;
                    rayPayload.surface.position = wspos;
                   
                }
            }
            ENDHLSL
        }
    }

}

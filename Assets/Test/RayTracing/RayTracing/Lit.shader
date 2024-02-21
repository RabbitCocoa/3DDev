Shader "Tutorial/SimpleLit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Main Color",Color) = (1,1,1,1)

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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }


    SubShader
    {
        Pass
        {
            Name "RayTracing"
            Tags
            {
                "LightMode" = "RayTracing"
            }

            HLSLPROGRAM
            #pragma raytracing test

            #include "./Common.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            float specular(float NdotH)
            {
                float t = 1 - 0.8 * NdotH * NdotH;
                t = t * t;
                return 0.1 / (PI * t);
            }

            [shader("closesthit")]
            void ClosestHitShader(inout RayPayload rayPayload : SV_RayPayload,
                                  AttributeData attributeData : SV_IntersectionAttributes)
            {
                if (rayPayload.rayType != RayTypeShadow)
                {
                    uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());
                    IntersectionVertex v0, v1, v2;
                    FetchIntersectionVertex(triangleIndices.x, v0);
                    FetchIntersectionVertex(triangleIndices.y, v1);
                    FetchIntersectionVertex(triangleIndices.z, v2);

                    float3 barycentricCoordinates = float3(
                        1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x,
                        attributeData.barycentrics.y);
                    float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS,
                                                                       barycentricCoordinates);
                    float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
                    float3 N = normalize(mul(objectToWorld, normalOS));


                    float3 origin = WorldRayOrigin();
                    float3 direction = WorldRayDirection();
                    float t = RayTCurrent();
                    float3 wspos = origin + direction * t; //命中点的世界目标
                    //diffuse
                    float L = normalize(float3(0.5, 0.5, 0.5));
                    float3 radiance = _Color.rgb * dot(N, L) / PI;

                    //specular
                    if (rayPayload.rayType != RayTypeReflection)
                    {
                        float3 V = -direction;
                        float3 H = normalize(L + V);
                        float NoH = dot(N, H);
                        radiance += specular(NoH) * _Color.rgb;
                    }
                    //shadow ray
                    RayDesc ray_desc;
                    ray_desc.Direction = L;
                    ray_desc.Origin = wspos + ray_desc.Direction * 0.001f;
                    ray_desc.TMax = 500;
                    ray_desc.TMin = 1e-5f;

                    RayPayload payload2;
                    payload2.rayType = RayTypeShadow;
                    payload2.hit = 1;
                    payload2.radiance = 0;

                    TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF,
                             0, 1, 0,
                             ray_desc,
                             payload2);

                    rayPayload.radiance = radiance * payload2.hit;

                    //二次反射
                    if (rayPayload.rayType == RayTypePrimary)
                    {
                        float reflectionDir = reflect(direction, N);

                        RayDesc ray_desc2;
                        ray_desc2.Direction = reflectionDir;
                        ray_desc2.Origin = wspos + ray_desc.Direction * 0.001f;
                        ray_desc2.TMax = 100;
                        ray_desc2.TMin = 1e-5f;

                        RayPayload payload3;
                        payload3.rayType = RayTypeReflection;
                        payload3.hit = 1;
                        payload3.radiance = 0;

                        TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF,
                                 0, 1, 0,
                                 ray_desc2,
                                 payload3);

                        rayPayload.radiance += payload3.radiance * max(0, dot(N, reflectionDir));
                    }
                }
                else
                {
                    rayPayload.hit = 0;
                }
            }
            ENDHLSL
        }
    }

    SubShader
    {
        Pass
        {
            Name "SoftShadow"
            Tags
            {
                "LightMode" = "RayTracing"
            }
            HLSLPROGRAM
            #pragma raytracing test
            #include"./Common.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            [shader("closesthit")]
            void ClosestHitShader(inout RayHitQuery rayPayload : SV_RayPayload,
                                  AttributeData attributeData : SV_IntersectionAttributes)
            {
                float3 origin = WorldRayOrigin();
                float3 direction = WorldRayDirection();
                float3 t = RayTCurrent();
                float3 wpos = origin + direction * t;

                if (rayPayload.rayType != RayTypeShadow)
                {
                    uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());
                    IntersectionVertex v0, v1, v2;
                    FetchIntersectionVertex(triangleIndices.x, v0);
                    FetchIntersectionVertex(triangleIndices.y, v1);
                    FetchIntersectionVertex(triangleIndices.z, v2);

                    float3 barycentricCoordinates = float3(
                        1 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x,
                        attributeData.barycentrics.y);
                    float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS,
                                                                       barycentricCoordinates);
                    float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
                    float3 N = normalize(mul(objectToWorld, normalOS));

                    rayPayload.albedo = _Color.rgb;
                    rayPayload.wspos = wpos;
                    rayPayload.normal = N;
                    rayPayload.hit = 1;
                }
                else
                {
                    rayPayload.hit = 1;
                    rayPayload.wspos = wpos;
                }
            }
            ENDHLSL
        }
    }
}
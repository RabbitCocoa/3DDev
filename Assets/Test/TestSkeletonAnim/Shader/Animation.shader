Shader "Unlit/Amimation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
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
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint vertexid:SV_VERTEXID;
            };

            float4x4 BoneAnimatedTransformArray[1023]; //local to World
            StructuredBuffer<float4> vertexBoneWeights;
            StructuredBuffer<int4> vertexBoneIndices;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                float4 weights = vertexBoneWeights[v.vertexid];
                int4 indices = vertexBoneIndices[v.vertexid];

                float3 vP = v.vertex.xyz;
                float3 vPacc = float3(0, 0, 0);

                vPacc += weights.x * mul(BoneAnimatedTransformArray[indices.x], float4(vP, 1)).xyz;
                vPacc += weights.y * mul(BoneAnimatedTransformArray[indices.y], float4(vP, 1)).xyz;
                vPacc += weights.z * mul(BoneAnimatedTransformArray[indices.z], float4(vP, 1)).xyz;
                vPacc += weights.w * mul(BoneAnimatedTransformArray[indices.w], float4(vP, 1)).xyz;

                v2f o;
                o.vertex = UnityObjectToClipPos (vPacc);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return _Color;
            }
            ENDCG
        }
    }
}
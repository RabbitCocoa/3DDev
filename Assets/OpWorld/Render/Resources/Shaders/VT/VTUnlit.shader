Shader "VirtualTexture/Unlit"
{
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "UnLit"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            CGPROGRAM
            #pragma vertex VTVert
            #pragma fragment VTFragUnlit
            #define VT_ON_SRP 0
            #include "VirtualTexture.hlsl"

            struct VTAppdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct VTV2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            VTV2f VTVert(VTAppdata v)
            {
                VTV2f o;
                UNITY_INITIALIZE_OUTPUT(VTV2f, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            CBUFFER_START(UnityPerMaterial)
                // per_material VT texture handle
                // 外部传入虚拟页面的xy
                float4 _VTPageHandle; // XY = vPageX, vPageY 
            CBUFFER_END


            fixed4 VTFragUnlit(VTV2f i) : SV_Target
            {
                VT_PAGE_TYPE inPageUV = VTSampleVirtualPage(_VTPageHandle, i.uv);
                fixed4 col = VTTex2D(inPageUV);
                return col;
            }
            ENDCG
        }


        
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"

}
Shader "VirtualTexture/Debug"
{
	SubShader
	{
		Tags{ "VirtualTextureType" = "Normal" }
			LOD 100

		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex VTVert
			#pragma fragment VTDebugMipmap
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

			fixed4 VTDebugMipmap(VTV2f i) : SV_Target
			{
				fixed4 pPage = tex2Dlod(_VTLookupTex, float4(i.uv, 0, 0)) * 255; //物理页表的x y
				int2 pageID = pPage.xy;
				if (pageID.x == INVALID_PAGE && pageID.y == INVALID_PAGE)
					return fixed4(0,0,0,1);
				
				pPage.xy /= PHYSICAL_PAGE_COUNT;
				return pPage;
			}
			ENDCG
		}


		Pass
		{
				CGPROGRAM
				#pragma vertex VTVert
				#pragma fragment VTFragDebug
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

				int _VTDebugTileParam;

				fixed4 VTFragDebug(VTV2f i) : SV_Target
				{
					i.uv.y = 1 - i.uv.y;  
				#if VT_TILE_TEXARRAY
					int2 pPageID = floor(i.uv * PHYSICAL_PAGE_COUNT);
					int index = pPageID.y * PHYSICAL_PAGE_X_COUNT + pPageID.x; //物理索引
					float2 inPageUV = frac(i.uv * PHYSICAL_PAGE_COUNT); //返回小数部分
					float3 uv = float3(inPageUV, index);
				#else
					float2 uv = i.uv;
				#endif	
					if (_VTDebugTileParam == 0) {
						return VTTex2D0(uv);
					}
					else if (_VTDebugTileParam == 1) {
						return VTTex2D1(uv);
					}
					else if (_VTDebugTileParam == 2) {
						return VTTex2D2(uv);
					}

					return float4(1, 0, 1, 1);
				}


				ENDCG
		}
	}
}

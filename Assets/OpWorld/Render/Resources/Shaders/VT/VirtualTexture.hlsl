#include <HLSLSupport.cginc>
#ifndef VIRTUAL_TEXTURE_INCLUDED
#define VIRTUAL_TEXTURE_INCLUDED

#define VT_TILE_TEXARRAY 1
//#define VT_ON_SRP 1
#if VT_ON_SRP
#define fixed4 half4
#endif

#define VPAGE_COUNT _VTVtableParam.x  //页表大小 n*n个虚拟页面
#define VPAGE_COUNT_RCP _VTVtableParam.y // 页面大小的倒数
#define VT_MAX_MIP_LEVEL _VTVtableParam.z //目前一直是0
#define VT_MIP_BIAS 0
#define INVALID_PAGE 255

#if VT_TILE_TEXARRAY
	#define VT_PAGE_TYPE float3
#else
	#define VT_PAGE_TYPE float2
#endif

#if VT_TILE_TEXARRAY
	#define PADDING_SIZE_OVER_PADDED_TILE_SIZE _VTTileParam.x   //纹理间 x的间隙 目前为0
	#define TILE_SIZE_OVER_PADDED_TILE_SIZE _VTTileParam.y     //纹理间 y的间隙 目前为0
	#define PHYSICAL_PAGE_X_COUNT _VTTileParam.z  //物理纹理的列数
	#define PHYSICAL_PAGE_Y_COUNT _VTTileParam.w //物理纹理的行数
	#define PHYSICAL_PAGE_COUNT _VTTileParam.zw
#else
	#define PADDING_SIZE_OVER_PADDED_TILE_SIZE _VTTileParam.x 
	#define TILE_SIZE_OVER_PADDED_TILE_SIZE _VTTileParam.y 
	#define PHYSICAL_PAGE_X_COUNT_RCP _VTTileParam.z 
	#define PHYSICAL_PAGE_Y_COUNT_RCP _VTTileParam.w
	#define PHYSICAL_PAGE_COUNT_RCP _VTTileParam.zw
#endif


// _VTFeedbackParam
// x: page size 虚拟页表大小
// y: virtual texture size 虚拟纹理大小
// z: max mipmap level 最大mip
// w: mipmap level bias 最小mip
float4 _VTFeedbackParam; 

// _VTVtableParam
// pageTable
// x: vtable 虚拟分页的数量
// y: 1 / vtable 虚拟分页的数量
float4 _VTVtableParam;

// _VTTileParam
// pagedTexture
// zw: 物理分页的数量
float4 _VTTileParam;


#if VT_ON_SRP
	TEXTURE2D(_VTLookupTex);
	SAMPLER(sampler_VTLookupTex);
	#define SAMPLE_VTLookupTex(vPageUV, mip) SAMPLE_TEXTURE2D_LOD(_VTLookupTex, sampler_VTLookupTex, vPageUV, mip)
#else
//页表纹理 外部传入
	sampler2D _VTLookupTex;
	#define SAMPLE_VTLookupTex(vPageUV, mip) tex2Dlod(_VTLookupTex, float4(vPageUV, 0, mip))
#endif


#if VT_TILE_TEXARRAY
//_VTTiledTexArray{i} 外部传入 见VirtualTexture
	#if VT_ON_SRP
		SamplerState sampler_LinearClamp;
		TEXTURE2D_ARRAY(_VTTiledTexArray0); SAMPLER(sampler_VTTiledTexArray0);
		TEXTURE2D_ARRAY(_VTTiledTexArray1); SAMPLER(sampler_VTTiledTexArray1);
		TEXTURE2D_ARRAY(_VTTiledTexArray2); SAMPLER(sampler_VTTiledTexArray2);
		TEXTURE2D_ARRAY(_VTTiledTexArray3); SAMPLER(sampler_VTTiledTexArray3);
		#define SAMPLE_TILED_TEX(tex2darray, uv)  SAMPLE_TEXTURE2D_ARRAY_LOD(tex2darray, sampler_LinearClamp, uv.xy, uv.z, 0);
	#else
		UNITY_DECLARE_TEX2DARRAY(_VTTiledTexArray0);
		UNITY_DECLARE_TEX2DARRAY(_VTTiledTexArray1);
		UNITY_DECLARE_TEX2DARRAY(_VTTiledTexArray2);
		UNITY_DECLARE_TEX2DARRAY(_VTTiledTexArray3);
		#define SAMPLE_TILED_TEX(tex2darray, uv)  UNITY_SAMPLE_TEX2DARRAY_LOD(tex2darray, uv, 0);
	#endif
#else
	sampler2D _VTTiledTex0;
	sampler2D _VTTiledTex1;
	sampler2D _VTTiledTex2;
	sampler2D _VTTiledTex3;
#endif

#define VT_PER_TEX_SIZE vtPageHandle.z
#define VT_PAGE_PER_TEX vtPageHandle.w

#if VT_TILE_TEXARRAY
// 采样虚拟页面 得到虚拟页面对应的物理页面
float3 VTSampleVirtualPage(in float4 vtPageHandle, in float2 uv)
{
	float2 vPageID = vtPageHandle.xy; // 例如 2，2  页表为4*4 
	float2 vPageUV = vPageID * VPAGE_COUNT_RCP; // 得到 2，2 /4 = 0.5 0.5 
	fixed4 pPage = SAMPLE_VTLookupTex(vPageUV, 0) * 255; //采样值乘以255得到对应页面 
	float2 inPageUV = uv;
	int index = pPage.y * PHYSICAL_PAGE_X_COUNT + pPage.x; //物理页面索引
	return float3(inPageUV, index); //u,v z:物理页面索引
}
#else
float2 VTSampleVirtualPage(in float4 vtPageHandle, in float2 uv)
{
	return 0;
}
#endif // VT_TILE_TEXARRAY


#if VT_TILE_TEXARRAY
fixed4 VTTex2D0(float3 uv)
{
	return SAMPLE_TILED_TEX(_VTTiledTexArray0, uv);
}

fixed4 VTTex2D1(float3 uv)
{
	return SAMPLE_TILED_TEX(_VTTiledTexArray1, uv);
}

fixed4 VTTex2D2(float3 uv)
{
	return SAMPLE_TILED_TEX(_VTTiledTexArray2, uv);
}

fixed4 VTTex2D(float3 uv)
{
	return VTTex2D0(uv);
}


#else
fixed4 VTTex2D0(float2 uv)
{
	return tex2D(_VTTiledTex0, uv);
}

fixed4 VTTex2D1(float2 uv)
{
	return tex2D(_VTTiledTex1, uv);
}

fixed4 VTTex2D2(float2 uv)
{
	return tex2D(_VTTiledTex2, uv);
}

fixed4 VTTex2D3(float2 uv)
{
	return tex2D(_VTTiledTex3, uv);
}

fixed4 VTTex2D(float2 uv)
{
	return VTTex2D0(uv);
}
#endif // VT_TILE_TEXARRAY

fixed4 VTGetMipColor(float mip)
{
	const fixed4 colors[12] = {
		fixed4(1, 0, 0, 1),
		fixed4(0.67, 0.1, 0.2, 1),
		fixed4(0.9, 0.44, 0.1, 1),
		fixed4(0.3, 0.1, 0.8, 1),

		fixed4(0.12, 0.9, 1.0, 1),
		fixed4(0.05, 0.9, 0.5, 1),
		fixed4(0.05, 1.0, 0.25, 1),
		fixed4(0, 1.0, 0, 1),

		fixed4(0.8, 0.1, 0.9, 1),
		fixed4(0, 0.25, 0.5, 1),
		fixed4(0.15, 0.2, 1, 1),
		fixed4(0.12, 0.6, 1, 1),
	};
	return colors[clamp(mip, 0, 11)];
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////
//	URP interface
////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if VT_ON_SRP
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

half4 VT_SampleAlbedoAlpha(VT_PAGE_TYPE inPageUV)
{
	return VTTex2D0(inPageUV);
}

half3 VT_SampleNormal(VT_PAGE_TYPE inPageUV, half scale = 1.0h)
{
#ifdef _NORMALMAP
		half4 n = VTTex2D1(inPageUV);
	#if BUMP_SCALE_NOT_SUPPORTED
		return UnpackNormal(n);
	#else
		return UnpackNormalScale(n, scale);
	#endif
#else
	return half3(0.0h, 0.0h, 1.0h);
#endif
}

half4 VT_SampleMetallicSmoothness(VT_PAGE_TYPE inPageUV)
{
	return VTTex2D2(inPageUV);
}

#endif // VT_ON_SRP



#endif // VIRTUAL_TEXTURE_INCLUDED
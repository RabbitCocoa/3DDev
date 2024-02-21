Shader "Custom/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("BlurSize", float) = 0.1
        _MaskMap ("MaskMap", 2D) = "black" {}
        _BlurredMap ("BlurredMap", 2D) = "black" {}
        _OutlineColor ("OutlineColor", color) = (1,1,1,1)
        _ColorIntensity ("ColorIntensity", float) = 1
    }


    HLSLINCLUDE

    
    #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

	TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
	TEXTURE2D(_MaskMap);SAMPLER(sampler_MaskMap);
	TEXTURE2D(_BlurredMap);SAMPLER(sampler_BlurredMap);
	float4 _MainTex_ST;
	float4 _OutlineColor;
	float2 _MainTex_TexelSize;
	half _BlurSize, _ColorIntensity;

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv :TEXCOORD0;
    };

    struct v2f
    {
        float4 positionCS : SV_POSITION;
        float2 texcoord:TEXCOORD0;
    };

    //记录像素点 及其上下的uv 
    struct v2f_DownSample
    {
        float4 vertex: SV_POSITION;
        float2 texcoord: TEXCOORD0;
        float2 uv: TEXCOORD1;
        float4 uv01: TEXCOORD2;
        float4 uv23: TEXCOORD3;
    };

    struct v2f_UpSample
    {
        float4 vertex: SV_POSITION;
        float2 texcoord: TEXCOORD0;
        float4 uv01: TEXCOORD1;
        float4 uv23: TEXCOORD2;
        float4 uv45: TEXCOORD3;
        float4 uv67: TEXCOORD4;
    };

v2f_DownSample Vert_DownSample(appdata v)
	{
		v2f_DownSample o;
		o.vertex = TransformObjectToHClip(v.vertex.xyz);
		o.texcoord = v.uv;
		
		float2 uv = TRANSFORM_TEX(o.texcoord, _MainTex);
		
		_MainTex_TexelSize *= 0.5;
		o.uv = uv;
		float2 _Offset = float2(1 + _BlurSize, 1 + _BlurSize);
		
		o.uv01.xy = uv - _MainTex_TexelSize * _Offset;//top right
		o.uv01.zw = uv + _MainTex_TexelSize * _Offset;//bottom left
		o.uv23.xy = uv - float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _Offset;//top left
		o.uv23.zw = uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _Offset;//bottom right
		
		return o;
	}
	
	half4 Frag_DownSample(v2f_DownSample i): SV_Target
	{
		half4 sum = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 4;
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw);
		
		return sum * 0.125;
	}
	
	
	v2f_UpSample Vert_UpSample(appdata v)
	{
		v2f_UpSample o;
		o.vertex = TransformObjectToHClip(v.vertex.xyz);
		o.texcoord = v.uv;
		
		float2 uv = TRANSFORM_TEX(o.texcoord, _MainTex);
		
		_MainTex_TexelSize *= 0.5;
		float2 _Offset = float2(1 + _BlurSize, 1 + _BlurSize);
		
		o.uv01.xy = uv + float2(-_MainTex_TexelSize.x * 2, 0) * _Offset;
		o.uv01.zw = uv + float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y) * _Offset;
		o.uv23.xy = uv + float2(0, _MainTex_TexelSize.y * 2) * _Offset;
		o.uv23.zw = uv + _MainTex_TexelSize * _Offset;
		o.uv45.xy = uv + float2(_MainTex_TexelSize.x * 2, 0) * _Offset;
		o.uv45.zw = uv + float2(_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _Offset;
		o.uv67.xy = uv + float2(0, -_MainTex_TexelSize.y * 2) * _Offset;
		o.uv67.zw = uv - _MainTex_TexelSize * _Offset;
		
		return o;
	}
	
	half4 Frag_UpSample(v2f_UpSample i): SV_Target
	{
		half4 sum = 0;
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw) * 2;
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw) * 2;
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv45.zw) * 2;
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv67.xy);
		sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv67.zw) * 2;
		
		return sum * 0.0833;
	}

	v2f Vert_SubtractAndAdd(appdata v)
	{
		v2f o;
		o.positionCS = TransformObjectToHClip(v.vertex.xyz);
		o.texcoord = v.uv;

		return o;
	}

	half4 Frag_SubtractAndAdd(v2f i):SV_Target
	{
		half maskMap = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, i.texcoord.xy);
		half4 bluredOutline = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord.xy);
	 	bluredOutline *= 1 - maskMap;
		 bluredOutline.a = bluredOutline.x;
		 bluredOutline.rgb *= _OutlineColor * _ColorIntensity;
		//
		return bluredOutline;
	}
    ENDHLSL

    SubShader
    {
		Cull Off ZWrite Off ZTest Always
		
		
		Pass
		{
			HLSLPROGRAM
			
			#pragma vertex Vert_DownSample
			#pragma fragment Frag_DownSample
			
			ENDHLSL
			
		}
		
		Pass
		{
			HLSLPROGRAM
			
			#pragma vertex Vert_UpSample
			#pragma fragment Frag_UpSample
			
			ENDHLSL
			
		}
		
		pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			
			HLSLPROGRAM

			#pragma vertex Vert_SubtractAndAdd
			#pragma fragment Frag_SubtractAndAdd
			
			ENDHLSL
		}
    }
}
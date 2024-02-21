#include "./Common.hlsl"

RWTexture2D<float4> _OutputTarget;
Texture2D<float4> _LightmapGbufferPosition;
Texture2D<float4> _LightmapGbufferNormal;
float4 _OutputTargetSize;
float _ShadowSoftness;
float4 _RNGSeed;
int _FrameIndex;
int _BounceCount;
float3 _SunDir;
float _SunIntensity;
TextureCube  _TexSkyLight;
SamplerState s_linear_clamp_sampler;
float _SkyLightIntensity;

struct BsdfSample
{
	float3 bsdf_dir;
	float3 bsdf;
	float pdf;
};


struct Bounce
{
	uint d;
	float3 throughput;
	RayDesc ray;
};


void sample_bxdf(
	in const Surface surface,
	in const Material material,
	in float3 V,
	out BsdfSample bsdf_sample,
	inout Random random
)
{
	bsdf_sample.bsdf_dir = 0;
	bsdf_sample.pdf = 0;
	bsdf_sample.bsdf = 0;

	float3 N = surface.ffnormal;
	//float3x3 tbn = get_tangent_space(N);
	
	float p_diffuse = 0, p_specular = 0;
	//random_pdf_lobe(surface, material, V, N, tbn, p_diffuse, p_specular);
	p_diffuse = 1;
	float u = rand(random);
	//if (u < p_diffuse)
	{
		float2 u2 = rand2(random);
		float3 L = importance_sample_cosweight(u2);
		L = tangent_to_world(L, surface.tangent, surface.bitangent, surface.ffnormal);
		L *= sign(dot(N, L));
		if (dot(N, L) > 0)
		{
			bsdf_sample.bsdf_dir = L;
			bsdf_sample.bsdf = material.albedo * INV_PI;
			bsdf_sample.pdf = abs(dot(N, L)) * INV_PI * p_diffuse;
		}
	}
	/*else if (u >= p_diffuse && u < p_diffuse + p_specular)
	{
		float2 u2 = rand2(random);
		float3 H = importance_sample_ggx(u2, material.roughness);
		H = tangent_to_world(H, tbn);
		float3 L = reflect(-V, H);
		if (dot(V, N) > 0 && dot(L, N) > 0) {  
			float3 specular = 0;
			float pdf = 0;
			evaluate_specular(material, L, V, N, specular, pdf);
			bsdf_sample.bsdf_dir = L;	
			bsdf_sample.bsdf = specular;
			bsdf_sample.pdf = pdf * p_specular;
		}
	}*/
}

void BounceOut(
	in const Surface surface,
	in const Material material,
	inout Bounce bounce,
	inout Random random
)
{
	float3 N = surface.ffnormal;
	float3 V = -bounce.ray.Direction;
	BsdfSample bsdf_sample;
	sample_bxdf(surface, material, V, bsdf_sample, random);
	if (bsdf_sample.pdf > 0)
	{
		float cosLi = abs(dot(bsdf_sample.bsdf_dir, N));
		bounce.ray.Direction = bsdf_sample.bsdf_dir;
		bounce.ray.Origin = surface.position;
		bounce.ray.TMin = 0.01;
		bounce.ray.TMax = 100;
		bounce.throughput *= bsdf_sample.bsdf * cosLi / bsdf_sample.pdf;
	}
	else {
		bounce.throughput = 0;
	}
}

float3 EvaluateLight(
	in const Surface surface,
	inout Random rng
)
{
  float3 radiance = 0;
	float3 wspos = surface.position;
	float3 N = surface.ffnormal;
  float3 L = -_SunDir;
  radiance = max(dot(N, L), 0) / PI * _SunIntensity;

  RayDesc shadowRayDescriptor;
  float3 fakeLightPos = wspos + L * 1000;
  float3 randPos = float3(rand(rng), rand(rng), rand(rng)) * 2 - 1;
  randPos *= _ShadowSoftness;
  shadowRayDescriptor.Direction = normalize(fakeLightPos + randPos - wspos);
  shadowRayDescriptor.Origin = wspos + N * 0.01;
  shadowRayDescriptor.TMin = 0;
  shadowRayDescriptor.TMax = 100;
  MetaRayPayload shadowPayload;
  shadowPayload.rayType = RayTypeShadow;
  shadowPayload.hit = 0;

  TraceRay(_AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, shadowRayDescriptor, shadowPayload);
  float shadow = shadowPayload.hit ? 0 : 1;
  radiance = radiance * shadow;
	return radiance;
}

float3 Raytracing(in const float3 wspos, in const float3 N, inout Random rng)
{
	float3 radiance = 0;
	Surface currentSurface;
	currentSurface.normal = N;
	currentSurface.ffnormal = N;
	currentSurface.position = wspos;
	float3 UpVector = abs(currentSurface.ffnormal.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
	currentSurface.tangent = normalize(cross(UpVector, currentSurface.ffnormal));
	currentSurface.bitangent = -normalize(cross(currentSurface.ffnormal, currentSurface.tangent));
	currentSurface.material.albedo = float3(1, 1, 1); // treat it as a white material, decouple the albedo

	RayDesc dummyRay; dummyRay.Direction = 0; dummyRay.Origin = 0; dummyRay.TMin = 0; dummyRay.TMax = 0;
	Bounce bounce = { 0, float3(1.f, 1.f, 1.f),  dummyRay };
	for (int i = 0; i < _BounceCount; ++i)
	{
		[branch] if (dot(bounce.throughput, bounce.throughput) < EPS) {
			break;
		}

		// sample light
		radiance += EvaluateLight(currentSurface, rng) * bounce.throughput;
		// shoot a bsdf sample
		BounceOut(currentSurface, currentSurface.material, bounce, rng);

		MetaRayPayload bounceRayload;
		bounceRayload.rayType = RayTypePrimary;
		bounceRayload.hit = 0;
		TraceRay(_AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, bounce.ray, bounceRayload);
		if (bounceRayload.hit)
		{
			currentSurface = bounceRayload.surface;
		}
		else
		{
			radiance += _TexSkyLight.SampleLevel(s_linear_clamp_sampler, bounce.ray.Direction, 0) * _SkyLightIntensity * bounce.throughput;
			break;
		}
	}
	return radiance;
}

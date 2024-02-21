#include "UnityRaytracingMeshUtils.cginc"
#include "./Random.hlsl"

#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };

RaytracingAccelerationStructure _AccelerationStructure;

static const float PI = 3.14159265358979323f;
static const float TWO_PI = 6.28318530717958648f;
static const float INV_PI = 1.f / PI;
static const float HALF_PI = 1.5707963267948966;
static const int RayTypePrimary = 0;
static const int RayTypeReflection = 1;
static const int RayTypeShadow = 2;
static const float EPS = 1e-5;


struct Material
{
	float3 albedo;
};

struct Surface
{
	float3 position;
	float3 normal;
	float3 ffnormal;
	float3 tangent;
	float3 bitangent;
	Material material;   // material
};

struct MetaRayPayload
{
  int rayType;
  Random rng;
  bool hit;
	Surface surface;
};

float3 tangent_to_world(
	in const float3 v, 
	in float3 tangent, in float3 bitangent, in float3 normal	
)
{
	// use [3x3]tbn mul [3x1]v
	return normalize(v.x * tangent + v.y * normal + v.z * bitangent);
}

float3 importance_sample_cosweight(float2 u)
{
	// u == sqr(sinTheta)
	float sinTheta = sqrt(u.x);
	float phi = 2.0 * PI * u.y;
	return float3(cos(phi) * sinTheta, sqrt(max(0.0, 1.0 - u.x)), sin(phi) * sinTheta);
}

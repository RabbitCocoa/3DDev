#ifndef COMMON_DEFINE
#define COMMON_DEFINE
#include "UnityRayTracingMeshUtils.cginc"
#include "Random.hlsl"

#define CBUFFER_START(name) cbuffer name{
#define CBUFFER_END };

static const float PI = 3.14159265358979323f;
static const float TWO_PI = 6.28318530717958648f;
static const float INV_PI = 1.f / PI;
static const float HALF_PI = 1.5707963267948966;
static const float EPS = 1e-5;

static const int RayTypePrimary = 0; //主射线
static const int RayTypeShadow = 1; //阴影射线 
static const int RayTypeReflection = 2; //阴影射线

struct Material
{
    float3 albedo;
};

struct Surface
{
    float3 normal;
    float3 ffnormal; //朝向外侧
    float3 position;
    float3 tangent;
    float3 bitangent;
    Material material;
};

struct MetaRayPayload
{
    int rayType;
    bool hit;
    Random rng;
    Surface surface;
};

float3 importance_sample_cosweight(float2 u)
{
    float sinTheta = sqrt(u.x);
    float phi = 2.0 * PI * u.y;
    return float3(cos(phi) * sinTheta, sqrt(max(0.0, 1.0 - u.x)), sin(phi) * sinTheta);
}

float3 tangent_to_world(float3 v, float3 tangent, float3 bitangent, float3 normal)
{
    return normalize(v.x * tangent + v.y * normal + v.z * bitangent);
}


#endif

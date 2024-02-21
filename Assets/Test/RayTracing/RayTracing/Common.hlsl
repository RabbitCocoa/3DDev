#include "UnityRayTracingMeshUtils.cginc"
#include "./Random.hlsl"
#define CBUFFER_START(name) cbuffer name{
#define CBUFFER_END };

CBUFFER_START(CameraBuff)
    float4x4 _InvCameraViewProj;
    float3 _WorldSpaceCameraPos;
    float _CameraFarDistance;
CBUFFER_END

RaytracingAccelerationStructure _AccelerationStructure;


#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

static const int RayTypePrimary = 0; //主射线
static const int RayTypeShadow = 1; //阴影射线 
static const int RayTypeReflection = 2; //阴影射线

struct IntersectionVertex
{
    float3 normalOS;
};

struct AttributeData
{
    float2 barycentrics;
};

struct RayPayload
{
    float3 radiance;
    int rayType;
    int hit;
};

struct RayHitQuery
{
    int rayType;
    float3 albedo;
    float3 wspos;
    float3 normal;
    Random rng;
    int hit;
};


static const float PI = 3.14159265358979323f;
static const float TWO_PI = 6.28318530717958648f;
static const float INV_PI = 1.f / PI;
static const float HALF_PI = 1.5707963267948966;




void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
    outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex,kVertexAttributeNormal);
}

inline void GenerateCameraRay(out float3 origin, out float3 direction)
{
    float2 xy = DispatchRaysIndex().xy + 0.5f; //中心在中间的像素 范围为 0~width


    //齐次空间uv坐标
    //xy =  uv*wh/2 + wh/2 = (uv+1)* (wh)/2
    // uv = 2*xy / wh - 1
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));
    world.xyz /= world.w;

    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}


inline void GenerateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset)
{
    float2 xy = DispatchRaysIndex().xy + float2(0.5, 0.5) + offset;
    float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;
    float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));
    world.xyz /= world.w;
    origin = _WorldSpaceCameraPos.xyz;
    direction = normalize(world.xyz - origin);
}

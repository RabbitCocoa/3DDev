
#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };


CBUFFER_START(CameraBuffer)
float4x4 _InvCameraViewProj;
float3 _WorldSpaceCameraPos;
float _CameraFarDistance;
CBUFFER_END


inline void GenerateCameraRay(out float3 origin, out float3 direction)
{
  float2 xy = DispatchRaysIndex().xy + 0.5f; 
  float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;
  float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));
  world.xyz /= world.w;
  origin = _WorldSpaceCameraPos.xyz;
  direction = normalize(world.xyz - origin);
}

inline void GenerateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset)
{
  float2 xy = DispatchRaysIndex().xy + float2(0.5,0.5) + offset;
  float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;
  float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));
  world.xyz /= world.w;
  origin = _WorldSpaceCameraPos.xyz;
  direction = normalize(world.xyz - origin);
}




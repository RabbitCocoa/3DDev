 #include "./Common.hlsl"
 #include "./RayTracingIntersection.hlsl"
 
 
 CBUFFER_START(UnityPerMaterial)
     half4 _BaseColor;
 CBUFFER_END
 
 [shader("closesthit")]
 void MetaPassCloestHit(inout MetaRayPayload rayPayload :SV_RayPayload,
                        AttributeData attributeData:SV_IntersectionAttributes)
 {
     float3 origin = WorldRayOrigin();
     float3 rayDirection = WorldRayDirection();
     float t = RayTCurrent();
     float3 wspos = origin + rayDirection * t;
 
     if (rayPayload.rayType != RayTypeShadow)
     {
         IntersectionVertex vertex;
         GetCurrentIntersectionVertex(attributeData, vertex);
         float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
         rayPayload.hit = true;
         rayPayload.surface.normal = normalize(mul(objectToWorld, vertex.normalOS));
         rayPayload.surface.ffnormal = dot(rayPayload.surface.normal, rayDirection) <= 0.0
                                           ? rayPayload.surface.normal
                                           : rayPayload.surface.normal * -1.0;
         rayPayload.surface.position = wspos;
 
         float3 UpVector = abs(rayPayload.surface.ffnormal.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
         rayPayload.surface.tangent = normalize(cross(UpVector, rayPayload.surface.ffnormal));
         rayPayload.surface.bitangent = -normalize(
             cross(rayPayload.surface.ffnormal, rayPayload.surface.tangent));
         rayPayload.surface.material.albedo = _BaseColor.rgb;
     }
     else
     {
         IntersectionVertex vertex;
         GetCurrentIntersectionVertex(attributeData, vertex);
         float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
         rayPayload.surface.normal = normalize(mul(objectToWorld, vertex.normalOS));
         rayPayload.hit = true;
         rayPayload.surface.position = wspos;
     }
 }
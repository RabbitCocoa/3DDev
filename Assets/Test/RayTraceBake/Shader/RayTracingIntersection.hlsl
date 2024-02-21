#ifndef RAYTACINGINTERSECTION_DEFINE
#define RAYTACINGINTERSECTION_DEFINE

struct AttributeData
{
    float2 barycentrics;
};
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)



struct IntersectionVertex
{
    float3 normalOS;
    float2 uv;
};

void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
    outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex,kVertexAttributeNormal);
    outVertex.uv =  UnityRayTracingFetchVertexAttribute2(vertexIndex,kVertexAttributeTexCoord0);
}

void GetCurrentIntersectionVertex(AttributeData attributeData, out IntersectionVertex outVertex)
{
    uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());
    IntersectionVertex v0, v1, v2;
    FetchIntersectionVertex(triangleIndices.x, v0);
    FetchIntersectionVertex(triangleIndices.y, v1);
    FetchIntersectionVertex(triangleIndices.z, v2);
    
    float3 barycentricCoordinates = float3(
                    1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x,
                    attributeData.barycentrics.y);
    
    float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS,
                                                                     barycentricCoordinates);
    float2 uv = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.uv, v1.uv, v2.uv,
                                                                   barycentricCoordinates);
    outVertex.normalOS = normalOS;
    outVertex.uv = uv;
}


#endif



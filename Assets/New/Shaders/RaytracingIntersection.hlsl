
#define INTERPOLATE_RAYTRACING_ATTRIBUTE(A0, A1, A2, BARYCENTRIC_COORDINATES) (A0 * BARYCENTRIC_COORDINATES.x + A1 * BARYCENTRIC_COORDINATES.y + A2 * BARYCENTRIC_COORDINATES.z)

struct AttributeData
{
  float2 barycentrics;
};

struct IntersectionVertex
{
    float3 normalOS;
};

void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
    outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
}


void GetCurrentIntersectionVertex(in AttributeData attributeData, out IntersectionVertex outVertex)
{
  uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());
  IntersectionVertex v0, v1, v2;
  FetchIntersectionVertex(triangleIndices.x, v0);
  FetchIntersectionVertex(triangleIndices.y, v1);
  FetchIntersectionVertex(triangleIndices.z, v2);

  float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);
  outVertex.normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
}



// /*****
// **文件:Node.cs
// **作者:
// **创建日期:21:34
// **说明:
// *****


using UnityEngine;

public class Node : MonoBehaviour
{
    public Node Parent;
    public Vector3 localPosition;
    public Vector3 localRotation;
    public Matrix4x4 locolToWorldMatrix = Matrix4x4.identity;


    //进行位置变换
    public void Transform()
    {
        Matrix4x4 localToParentMatrix4X4 = Matrix4x4.identity;
        localToParentMatrix4X4.SetTRS(localPosition,Quaternion.Euler(localRotation),Vector3.one);
        locolToWorldMatrix = Parent.locolToWorldMatrix * localToParentMatrix4X4;
        transform.position = locolToWorldMatrix * new Vector4(0,0,0,1);
        transform.rotation = GetRotation(locolToWorldMatrix);

    }
    
    Quaternion GetRotation(Matrix4x4 matrix4X4)
    {
        float qw = Mathf.Sqrt(1f + matrix4X4.m00 + matrix4X4.m11 + matrix4X4.m22) / 2;
        float w = 4 * qw;
        float qx = (matrix4X4.m21 - matrix4X4.m12) / w;
        float qy = (matrix4X4.m02 - matrix4X4.m20) / w;
        float qz = (matrix4X4.m10 - matrix4X4.m01) / w;
        return new Quaternion(qx, qy, qz, qw);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f));
    }
}
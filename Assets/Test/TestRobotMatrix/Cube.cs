// /*****
// **文件:Cube.cs
// **作者:
// **创建日期:22:22
// **说明:
// *****

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace OpWorld.TestRobotMatrix
{
    //网格
    public class Cube : MonoBehaviour
    {
        [FormerlySerializedAs("Node")] public MNode mNode; //骨骼
        // public Vector3 localPosition;
        // public Vector3 localRotation;
        // public Vector3 localScale = Vector3.one;

        private Matrix4x4 localToWorldMatrix_Oreginal;  //不变的 初始赋值
        private Matrix4x4 localToWorldMatrix; //最终的变换矩阵 是动态的

        public void initMatrix()
        {
            localToWorldMatrix_Oreginal = transform.localToWorldMatrix;
        }

        public void TransformCube()
        {
            Matrix4x4 loccalPosRotmatrix = Matrix4x4.identity;
         //   loccalPosRotmatrix.SetTRS(localPosition, quaternion.Euler(localRotation), localScale);

         localToWorldMatrix = mNode.localToWorldMatrix * mNode.invBindMatrix * //骨骼的变换
                              localToWorldMatrix_Oreginal; //;网格自身不会动* loccalPosRotmatrix;  //自身的世界坐标
            transform.position = localToWorldMatrix.GetPosition();
            transform.rotation = localToWorldMatrix.rotation;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.2f, 0.3f, 0.3f));
        }
    }
}
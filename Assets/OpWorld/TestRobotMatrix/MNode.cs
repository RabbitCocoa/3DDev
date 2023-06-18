// /*****
// **文件:Node.cs
// **作者:
// **创建日期:22:06
// **说明:
// *****

using System;
using UnityEngine;

namespace OpWorld.TestRobotMatrix
{
    //相当于骨骼
    public class MNode : MonoBehaviour
    {
        public Transform parent;
        public Vector3 localPosition;
        public Vector3 localRotation;


        //本地到世界的变换矩阵
        //初始状态下
        public Matrix4x4 localToParentMatrix;
        public Matrix4x4 localToWorld_Original;
        public Matrix4x4 invBindMatrix; //世界坐标到本地坐标的变换矩阵

   

        //运动后的
        public  Matrix4x4 localToWorldMatrix;

        public void initMatrix()
        {
            localToWorld_Original = transform.localToWorldMatrix;
            localToParentMatrix = parent.localToWorldMatrix.inverse * localToWorld_Original;
            invBindMatrix = localToWorld_Original.inverse;
        }

        public void TransformNode()
        {
            Matrix4x4 localRSTMatrix = Matrix4x4.identity;
            localRSTMatrix.SetTRS(localPosition, Quaternion.Euler(localRotation), Vector3.one);
            localToWorldMatrix = parent.localToWorldMatrix * localToParentMatrix * localRSTMatrix;
            
            transform.position = localToWorldMatrix.GetPosition();
            transform.rotation = localToWorldMatrix.rotation;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f));
        }
    }
}
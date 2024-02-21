// /*****
// **文件:AnimatedMesh.cs
// **作者:
// **创建日期:13:58
// **说明: 蒙皮
// *****


using System.Collections.Generic;
using UnityEngine;

namespace Anim
{
    public class AnimatedMesh
    {
        private Mesh mesh;
        private List<Vector3> _srcVertices;
        public Matrix4x4[] BoneAnimatedTransformArray = null; // 当前帧 骨骼的变化矩阵
        private Skeleton _skeleton;
        private List<Vector3> _newPoints; //新的顶点

        public ComputeBuffer boneWeightBuffer;
        public ComputeBuffer boneInciesBuffer;
        
        public AnimatedMesh(Mesh mesh)
        {
            this.mesh = mesh;
            _srcVertices = new List<Vector3>();
            this.mesh.GetVertices(_srcVertices);
            _newPoints = new List<Vector3>(_srcVertices);
            
            var _weights = mesh.boneWeights;
            boneWeightBuffer = new ComputeBuffer(_weights.Length, 4 * sizeof(float));

            boneInciesBuffer = new ComputeBuffer(_weights.Length, 4 * sizeof(int));

            List<float> weights = new List<float>();

            List<int> incies = new List<int>();

            foreach (BoneWeight boneWeight in mesh.boneWeights) {
                weights.Add(boneWeight.weight0);
                weights.Add(boneWeight.weight1);
                weights.Add(boneWeight.weight2);
                weights.Add(boneWeight.weight3);
                incies.Add(boneWeight.boneIndex0);
                incies.Add(boneWeight.boneIndex1);
                incies.Add(boneWeight.boneIndex2);
                incies.Add(boneWeight.boneIndex3);
            }


            boneWeightBuffer.SetData(weights.ToArray());
            boneInciesBuffer.SetData(incies.ToArray());
        }

        public void SetSkeleton(Skeleton skeleton)
        {
            _skeleton = skeleton;
            BoneAnimatedTransformArray = new Matrix4x4[_skeleton.BoneCount];
        }

        public void Render()
        {
            var weights = mesh.boneWeights;
            for (int i = 0; i < _srcVertices.Count; i++)
            {
                Vector3 vertex = _srcVertices[i];
                ref BoneWeight weight = ref weights[i];

                //第i个节点的权重
                //计算每个骨骼位移对其的变化矩阵
                ref Matrix4x4 boneLocalToRoot0 = ref BoneAnimatedTransformArray[weight.boneIndex0];
                ref Matrix4x4 boneLocalToRoot1 = ref BoneAnimatedTransformArray[weight.boneIndex1];
                ref Matrix4x4 boneLocalToRoot2 = ref BoneAnimatedTransformArray[weight.boneIndex2];
                ref Matrix4x4 boneLocalToRoot3 = ref BoneAnimatedTransformArray[weight.boneIndex3];

                Vector3 animtedVertex = boneLocalToRoot0.MultiplyPoint(vertex) * weight.weight0 +
                                        boneLocalToRoot1.MultiplyPoint(vertex) * weight.weight1 +
                                        boneLocalToRoot2.MultiplyPoint(vertex) * weight.weight2 +
                                        boneLocalToRoot3.MultiplyPoint(vertex) * weight.weight3;
                //得到是一个基于父节点的位移
                _newPoints[i] = animtedVertex;
            }
            mesh.SetVertices(_newPoints);
        }
    }
}
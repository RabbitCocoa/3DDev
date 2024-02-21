// /*****
// **文件:Skeleton.cs
// **作者:
// **创建日期:13:53
// **说明:骨骼
// *****


using UnityEngine;

namespace Anim
{
    public class Skeleton
    {
        private readonly Matrix4x4[] bindPos; //模型相对于Root的变化矩阵
        public int BoneCount { get; }

        public Skeleton(Matrix4x4[] bindPose)
        {
            bindPos = bindPose;
            BoneCount = bindPose.Length;
        }

        public Matrix4x4 GetBindMatrix(int index)
        {
            return bindPos[index];
        }
    }
}
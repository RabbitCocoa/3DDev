// /*****
// **文件:SkeletonAnimator.cs
// **作者:
// **创建日期:13:57
// **说明:
// *****

using UnityEngine;

namespace Anim
{
    public class SkeletonAnimator
    {
        private float _time;
        private int _frame;

        public AnimData _animData;

        public SkeletonAnimator()
        {
            _time = 0;
            _frame = 0;
        }

        public void Animate(AnimatedMesh mesh, Skeleton skeleton)
        {
            if (_animData == null)
                return;
            if (_frame < 0)
            {
                ApplyFrame(0, mesh, skeleton);
                return;
            }
            _time += Time.deltaTime;
            _time %= _animData.animLen;
            int f = (int)(_time / (1.0f / _animData.frame)); //计算当前是第几帧
            if (f != _frame) {
                ApplyFrame(f, mesh, skeleton);
            }
        }
        
        //克制第n帧的蒙皮
        public void ApplyFrame(int frame, AnimatedMesh mesh, Skeleton skeleton)
        {
            _frame = frame;

            if (skeleton == null || mesh == null)
                return;

            for (int boneIndex = 0; boneIndex < skeleton.BoneCount; ++boneIndex) {
                AnimData.FrameData frameData = _animData.frameDatas[frame];
                                                                //                   Root to World   *该帧的变换矩阵*初始状态 相对于root的变换矩阵
                mesh.BoneAnimatedTransformArray[boneIndex] = frameData.animtedLocalToRoot[boneIndex] * skeleton.GetBindMatrix(boneIndex); 
            }
        }
    }
}
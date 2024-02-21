// /*****
// **文件:CustomAnim.cs
// **作者:cocoa
// **创建日期:14:39
// **说明:
// *****

using UnityEngine;

namespace Anim
{
    public class CustomAnim : MonoBehaviour
    {
        Skeleton _skeleton;
        SkeletonAnimator _animator;
        public AnimData _animData;
        AnimatedMesh _skinMesh;

        [ContextMenu("Reset")]
        public void Reset()
        {
            var mesh = GetComponentInChildren<MeshFilter>().sharedMesh;
            _skinMesh = new AnimatedMesh(mesh); //蒙皮
            _skeleton = new Skeleton(mesh.bindposes);
            _skinMesh.SetSkeleton(_skeleton);
            _animator = new SkeletonAnimator();
            
            _animator._animData = _animData;
            _animator.ApplyFrame(0,_skinMesh,_skeleton);
            _skinMesh.Render();
        }
        void Awake()
        {
            
            var mesh = GetComponentInChildren<MeshFilter>().mesh;
            _skinMesh = new AnimatedMesh(mesh); //蒙皮
            _skeleton = new Skeleton(mesh.bindposes);
            _skinMesh.SetSkeleton(_skeleton);
            _animator = new SkeletonAnimator();
            Play();
        }
        
        void Play()
        {
            _animator._animData = _animData;
        }

        void Update()
        {
            _animator.Animate(_skinMesh, _skeleton); //根据骨骼和蒙皮 设置当前帧的变换矩阵
            _skinMesh.Render(); //根据骨骼的变换和权重 计算出每个顶点的权重
        }

    }
}
// /*****
// **文件:CustomGPUAnim.cs
// **作者:cocoa
// **创建日期:14:52
// **说明:
// *****

using System;
using UnityEngine;

namespace Anim
{
    public class CustomGPUAnim : MonoBehaviour
    {
        Skeleton _skeleton;
        SkeletonAnimator _animator;
        public AnimData _animData;
        AnimatedMesh _skinMesh;
        void Awake()
        {
            var mesh = GetComponentInChildren<MeshFilter>().mesh;
            _skinMesh = new AnimatedMesh(mesh);
            _skeleton = new Skeleton(mesh.bindposes);
            _skinMesh.SetSkeleton(_skeleton);
            _animator = new SkeletonAnimator();
            Play();
        }
        void Play()
        {
            _animator._animData = _animData;
            Shader.SetGlobalBuffer("vertexBoneWeights", _skinMesh.boneWeightBuffer);
            Shader.SetGlobalBuffer("vertexBoneIndices", _skinMesh.boneInciesBuffer);
        }
        
        void Update()
        {
            _animator.Animate(_skinMesh, _skeleton);
        }

        private void OnDestroy()
        {
            _animator.ApplyFrame(0,_skinMesh,_skeleton);
            OnWillRenderObject();
        }

        private void OnWillRenderObject()
        {
            Shader.SetGlobalMatrixArray("BoneAnimatedTransformArray", _skinMesh.BoneAnimatedTransformArray);
             
        }
      
    }
}
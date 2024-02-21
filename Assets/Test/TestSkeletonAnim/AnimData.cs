// /*****
// **文件:AnimaData.cs
// **作者:
// **创建日期:13:55
// **说明: 动画数据
// *****

using System;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;

namespace Anim
{
    public class AnimData : ScriptableObject
    {
        [Serializable]
        public class FrameData
        {
            public float time;
            public Matrix4x4[] animtedLocalToRoot; //模型变换到世界的矩阵 即    localToWorld* SRT 
        }

        public string animName;

        public float animLen;

        public int frame;

        public FrameData[] frameDatas;


    }
}
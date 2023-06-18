// /*****
// **文件:Root.cs
// **作者:
// **创建日期:22:19
// **说明:
// *****

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.TestRobotMatrix
{
    public class MRoot : MonoBehaviour
    {
        public MNode[] Nodes;
        public Cube[] Cube;
        
        [Serializable]
        public class FrameData
        {
            public List<Vector3> locolPos;
            public List<Vector3> localRot;
        }     
        [Range(0,1)]
        public float time;

        public FrameData[] data = new FrameData[2];
        
        
        void Start()
        {
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (i == 0)
                    Nodes[i].parent = transform;
                else
                {
                    Nodes[i].parent = Nodes[i - 1].transform;
                }
                Nodes[i].initMatrix();

            }
            for (int i = 0; i < Cube.Length; i++)
            {
                Cube[i].mNode = Nodes[i];
                Cube[i].initMatrix();
            }
        }

        private void Update()
        {
            for (int i = 0; i < Nodes.Length; i++)
            {
                Nodes[i].localPosition = Vector3.Lerp( data[0].locolPos[i],data[1].locolPos[i],time);
                Nodes[i].localRotation = Vector3.Lerp(data[0].localRot[i], data[1].localRot[i], time);
                Nodes[i].TransformNode();
            }

            for (int i = 0; i < Cube.Length; i++)
            {
                Cube[i].TransformCube();
            }
        }
        
        [ContextMenu("标记为当前帧位置")]
        public void RecordCurrent()
        {
            data[0].locolPos = new List<Vector3>();
            data[0].localRot = new List<Vector3>();
            for (int i = 0; i < Nodes.Length; i++)
            {
                data[0].locolPos.Add(Nodes[i].localPosition);
                data[0].localRot.Add(Nodes[i].localRotation);
            }
        }
        
        [ContextMenu("标记为下一帧位置")]
        public void RecordNext()
        {
            data[1].locolPos = new List<Vector3>();
            data[1].localRot = new List<Vector3>();
            for (int i = 0; i < Nodes.Length; i++)
            {
                data[1].locolPos.Add(Nodes[i].localPosition);
                data[1].localRot.Add(Nodes[i].localRotation);
            }
        }
    }
}
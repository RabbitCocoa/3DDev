// /*****
// **文件:Root.cs
// **作者:
// **创建日期:21:42
// **说明:
// *****

using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.TestRobot
{
    public class Root : Node
    {
        public List<Node> Nodes;
        public List<Vector3> locolPos;
        public List<Vector3> localRot;

    
   
        void Start()
        {
            this.locolToWorldMatrix = transform.localToWorldMatrix;

        }
 
        void Update()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (i == 0)
                {
                    Nodes[i].Parent =  this;
                }
                else
                {
                    Nodes[i].Parent = Nodes[i - 1];
                }
                Nodes[i].localPosition = locolPos[i];
                Nodes[i].localRotation = localRot[i];
                Nodes[i].Transform();
            }
        }


     
    }
}
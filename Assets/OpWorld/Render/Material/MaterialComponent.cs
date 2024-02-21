using System;
using OpWorld.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Render
{
    public class MaterialComponent : MonoBehaviour, IComponentBlueprintInstance
    {
        public Vector4 vtHandle = new Vector4(1,3);

        private Material _material;
        public string texName;
        
        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            
        }

        private void OnEnable()
        {
            
            //通过texName 加载对应的纹理图
            //申请一个物理页表 分配
            
        }

        private void OnDestroy()
        {
            //将虚拟页表指向非法
        }

        private void Update()
        {
            _material.SetVector("_VTPageHandle",vtHandle);
        }

        public void Apply(IComponentBlueprint bp)
        {
            MaterialComponentBlueprint materialBp = (MaterialComponentBlueprint)bp;
            vtHandle = materialBp.VTHandle;
            texName = materialBp.MatName;
        }
    }
}
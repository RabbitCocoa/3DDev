using OpWorld.Core;
using OpWorld.Render.VT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OpWorld.Render
{
    public class MaterialComponentBlueprint : MonoBehaviour, IComponentBlueprint
    {
        private Material material;
        private Vector4 vtHandle;

        private string matName;
        
        public Vector4 VTHandle
        {
            get => vtHandle;
        }

        public string MatName => matName;

        public void BuildComponentBlueprint(ComponentBlueprintQueue queue)
        {
            queue.Add(priority: 0, () =>
            {
                VirtualTextureBlueprint vt = GameObject.FindObjectOfType<VirtualTextureBlueprint>();
                vtHandle = vt.GetVTHandle();
                vt.Save(vtHandle, material.mainTexture);
            });
        }

        public void CreateInstance(GameObject gameObject)
        {
            //添加MaterialComponent组件
            throw new System.NotImplementedException();
        }
    }
}
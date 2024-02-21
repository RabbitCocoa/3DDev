using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Render.VT
{
    // VirtualTextureBlueprint

    // GetVTHandle 计算 VTHandle

    // 虚拟贴图，他是知道 VTHandle 代表的贴图，保存的位置

    // 保存贴图，StreamingAssets/asset bundle

    public class VirtualTextureBlueprint : MonoBehaviour
    {
        public int tableSize = 4;
        private int pageSize = 512;

        private int INVALID_PAGE = 255;
        private int alloc_x;
        public int alloc_y;
        public Vector4 GetVTHandle()
        {
            Vector4 vtHandle = new Vector4(alloc_x, alloc_y);
            alloc_y = (alloc_y + 1) % tableSize;
            if (alloc_y == 0)
                alloc_x++;
            if (alloc_x >= tableSize)
            {
                Debug.LogError($"页数分配过少");
                return new Vector4(INVALID_PAGE,INVALID_PAGE,0,0);
            }
            return vtHandle;
        }

        public string Save(Vector4 vtHandle, Texture texture)
        {
            //将纹理存入对应的文件夹
            return "";
        }

    }
}
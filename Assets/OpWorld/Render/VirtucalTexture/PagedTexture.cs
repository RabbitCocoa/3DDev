using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Render.VT
{
    //整个虚拟纹理贴图
    public class PagedTexture
    {
        // 整个 PagedTexture 贴图的分辨率大小，比如 xy = 4096 x 4096
        private Vector2Int textureSize = default;
        private int pageSize = 256; //分页大小

        private Vector2Int pageCount = default;

        // 贴图格式
        private TextureFormat format;

        private bool isLinear;

// 存放每个贴图分页的地方，是一个Texture2D数组
        private Texture2DArray texArray;
        
        public PagedTexture(Vector2Int textureSize, int pageSize, TextureFormat format, bool isLinear)
        {
            this.textureSize = textureSize;
            this.pageSize = pageSize;
            this.format = format;
            this.isLinear = isLinear;
        }
        // 每当分页刷新时的动作
        public event Action<Vector2Int> OnPageUpdateComplete;
        
        public Vector2Int TextureSize { get => textureSize; }
        public void SetSize(Vector2Int size) { textureSize = size; }
        public int PageSize { get { return pageSize; } }

        public Vector2Int PageCount { get => pageCount;  }

        public void SetPageSize(int size) => pageSize = size;
        public Texture GetTexture()  => texArray;

        public void Create()
        {
            pageCount = new Vector2Int(textureSize.x / pageSize, textureSize.y / pageSize);
            int length = pageCount.x * pageCount.y;
            texArray = new Texture2DArray(
                PageSize,
                PageSize,
                length,
                format,
                1,
                isLinear);
            texArray.wrapMode = TextureWrapMode.Clamp;
        }
        public void Destroy()
        {
            if (texArray != null)
                UnityEngine.Object.Destroy(texArray);
        }
        //索引ID转化为 二维xy
        public Vector2Int IDtoPos(int pageID)
        {
            return new Vector2Int(pageID % pageCount.x, pageID / pageCount.x);
        }
        //二维xy转化为对应索引位置
        public int PosToId(Vector2Int pageID)
        {
            return (pageID.y * pageCount.x + pageID.x);
        }
        //设置对应物理页面的贴图
        public void RenderPage(Vector2Int pageID, Texture2D texture2D)
        {
            if (texture2D == null)
                return;

            RenderPage(pageID, texture2D, texArray);
        }
        private void RenderPage(Vector2Int pageID, Texture2D source, Texture2DArray target)
        {
            if (source == null || target == null)
                return;
            int index = PosToId(pageID);
            Graphics.CopyTexture(source, 0, 0, target, index, 0);
        }
    }
}
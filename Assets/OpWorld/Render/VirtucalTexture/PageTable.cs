using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OpWorld.Render.VT
{
    //虚拟分页
    public class VirtualPage
    {
        public static int INVALID_PAGE = 255;

        //页表
        private PageTable _pageTable;

        //虚拟页面ID
        public Vector2Int pageID;

        //对应的物理页面ID
        public Vector3Int pageData;

        public VirtualPage(PageTable pageTable, Vector2Int pageID)
        {
            this._pageTable = pageTable;
            this.pageID = pageID;
        }

        public bool IsValid => pageData.x != INVALID_PAGE && pageData.y != INVALID_PAGE;

        public void Set(int pageX, int pageY)
        {
            pageData = new Vector3Int(pageX, pageY, 0);
            PageUploader.UploadPage(_pageTable.LookupTexture, pageID, 0, pageData);
        }

        //指向无效的物理分页
        public void SetInvliad()
        {
            pageData = new Vector3Int(INVALID_PAGE, INVALID_PAGE, 0);
        }
    }

    // 页表
    // 存储所有的虚拟分页
    // 同步 所有的虚拟分页数据到GPU
    // 内存、GPU 
    public class PageTable
    {
        //页表大小 x * x
        private int tableSize;

        private RenderTexture lookupTexture;


        private VirtualPage[] pages;


        public int TableSize
        {
            get { return tableSize; }
        }

        public RenderTexture LookupTexture
        {
            get => lookupTexture;
            set => lookupTexture = value;
        }

        public RenderTexture DebugTexture { get; private set; }

        public PageTable(int tableSize)
        {
            this.tableSize = tableSize;
        }

        public void Create()
        {
            InitPageTable(tableSize);
            InitPageTableTexture();
            //InitDebugTexture(320, 320);
            //设置页表纹理 
            ResetPageTableTexture(0);
        }

        public void Destroy()
        {
            if (lookupTexture != null)
                lookupTexture.Release();
        }

        //初始化虚拟页面
        private void InitPageTable(int tableSize)
        {
            pages = new VirtualPage[tableSize * tableSize];
            for (int i = 0; i < tableSize; ++i)
            {
                for (int j = 0; j < tableSize; ++j)
                {
                    VirtualPage page = new VirtualPage(this, new Vector2Int(j, i));
                    page.SetInvliad();
                    pages[i * tableSize + j] = page;
                }
            }
        }

        //初始化物理纹理
        private void InitPageTableTexture()
        {
            var desc = new RenderTextureDescriptor()
            {
                width = TableSize,
                height = TableSize,
                dimension = TextureDimension.Tex2D,
                colorFormat = RenderTextureFormat.ARGB32, // int 4
                sRGB = false,
                volumeDepth = 1,
                useMipMap = true,
                autoGenerateMips = false,
                mipCount = 1,
                msaaSamples = 1,
                enableRandomWrite = true,
            };
            lookupTexture = new RenderTexture(desc);
            lookupTexture.filterMode = FilterMode.Point;
            lookupTexture.Create();
        }

        //取得虚拟页面
        public VirtualPage FetchPage(int x, int y)
        {
            return pages[y * tableSize + x];
        }

        private void ResetPageTableTexture(int mipLevel)
        {
            Vector4[] pixels = new Vector4[tableSize * tableSize];
            for (int y = 0; y < tableSize; ++y)
            {
                for (int x = 0; x < tableSize; ++x)
                {
                    var page = FetchPage(x, y);
                    var id = y * tableSize + x;
                    var c = new Vector4(page.pageData.x, page.pageData.y, 0, 255) / 255.0f;
                    pixels[id] = c;
                }
            }

            UploadPageTable(tableSize, tableSize, mipLevel, pixels);
        }

        private void UploadPageTable(int pageCountX, int pageCountY, int mipLevel, Vector4[] pixels)
        {
            Texture2D indirectTex = new Texture2D(pageCountX, pageCountY, TextureFormat.RGBAFloat, 1, true);
            indirectTex.filterMode = FilterMode.Point;
            indirectTex.wrapMode = TextureWrapMode.Clamp;
            indirectTex.SetPixelData(pixels, 0);
            indirectTex.Apply(false);
            PageUploader.UploadPageTableTexture(indirectTex, lookupTexture, pageCountX, pageCountY, mipLevel);
        }
    }

    public class PageUploader
    {
        private static PageUploader g_PageUploader = new PageUploader();

        private ComputeShader m_uploadPageShader;
        private int m_uploadPageShaderKernel;
        private int m_uploadTexShaderKernel;

        public PageUploader()
        {
            m_uploadPageShader = Resources.Load("Shaders/VT/UploadPageTable") as ComputeShader;
            m_uploadPageShaderKernel = m_uploadPageShader.FindKernel("UploadPageCS");
            m_uploadTexShaderKernel = m_uploadPageShader.FindKernel("UploadTexCS");
        }

        //上传虚拟页面数据
        // 设置单个虚拟页面纹理
        
        private void UploadPageImpl(RenderTexture indirectTex, in Vector2Int pageID, in int mipLevel,
            in Vector3Int pageData)
        {
            var offset = new Vector4(pageID.x, pageID.y, 0, 0);
            var data = new Vector4(pageData.x, pageData.y, pageData.z, 255) / 255.0f;
            m_uploadPageShader.SetTexture(m_uploadPageShaderKernel, "_PageTableRT", indirectTex, mipLevel);
            m_uploadPageShader.SetVector("_VTPageOffset", offset);
            m_uploadPageShader.SetVector("_VTPageValue", data);
            m_uploadPageShader.Dispatch(m_uploadPageShaderKernel, 1, 1, 1);
        }

        //设置整个虚拟页面纹理 
        private void UploadPageTableTextureImpl(Texture2D srcTex, RenderTexture indirectTex, int pageCountX,
            int pageCountY, int mipLevel)
        {
            m_uploadPageShader.SetTexture(m_uploadTexShaderKernel, "_InputTex", srcTex);
            m_uploadPageShader.SetTexture(m_uploadTexShaderKernel, "_PageTableRT", indirectTex, mipLevel);
            int groupX = Mathf.CeilToInt(pageCountX / 16.0f);
            int groupY = Mathf.CeilToInt(pageCountY / 16.0f);
            m_uploadPageShader.Dispatch(m_uploadTexShaderKernel, groupX, groupY, 1);
        }

        static public void UploadPage(RenderTexture indirectTex, in Vector2Int pageID, in int mipLevel,
            in Vector3Int pageData)
        {
            g_PageUploader.UploadPageImpl(indirectTex, pageID, mipLevel, pageData);
        }

        static public void UploadPageTableTexture(Texture2D srcTex, RenderTexture indirectTex, int pageCountX,
            int pageCountY, int mipLevel)
        {
            g_PageUploader.UploadPageTableTextureImpl(srcTex, indirectTex, pageCountX, pageCountY, mipLevel);
        }
    }
}
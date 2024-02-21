using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Render.VT
{
    //虚拟贴图 管理页表和
    //巨大贴图
    public class VirtualTexture : MonoBehaviour
    {
        //页表大小
        // 页表的大小 如 64x64
        private int tableSize = 4;

        // 整个 PagedTexture 贴图的分辨率大小，比如 xy = 4096 x 4096
        private Vector2Int textureSize = new Vector2Int(4 * 512, 4 * 512);

        // 每个分页贴图大小, 如 256x256
        private int pageSize = 512;
        private TextureFormat format = TextureFormat.RGBA32;
        public bool enableDebug = false;
        public bool show = true;


        //页表
        private PageTable pageTable;

        //虚拟纹理
        private PagedTexture _pagedTexture;

        // x,y=》 虚拟页表
        private Dictionary<Vector2Int, VirtualPage> pageMapping = new Dictionary<Vector2Int, VirtualPage>();


        private VTDebugger debuger;

        public PageTable PageTable
        {
            get => pageTable;
        }

        public PagedTexture PagedTexture
        {
            get => _pagedTexture;
        }

        private RenderTexture debugPageTableTexture { get; set; }
        private RenderTexture[] debugPageTextures { get; set; }


        private void Awake()
        {
            pageTable = new PageTable(tableSize);
            pageTable.Create();

            _pagedTexture = new PagedTexture(textureSize, pageSize, format, false);
            _pagedTexture.Create();

            debuger = new VTDebugger(this);
            InitDebugTexture(320, 320);
        }

        private void OnDestroy()
        {
            pageTable.Destroy();
            _pagedTexture.Destroy();
            debugPageTableTexture.Release();
            for (int i = 0; i < debugPageTextures.Length; i++)
            {
                debugPageTextures[i].Release();
            }
        }

        private void InitDebugTexture(int w, int h)
        {
            debugPageTableTexture = new RenderTexture(w, h, 0);
            debugPageTableTexture.wrapMode = TextureWrapMode.Clamp;
            debugPageTableTexture.filterMode = FilterMode.Point;

            int layerCount = 1;
            debugPageTextures = new RenderTexture[layerCount];
            for (int i = 0; i < layerCount; i++)
            {
                debugPageTextures[i] = new RenderTexture(w, h, 0);
                debugPageTextures[i].useMipMap = false;
                debugPageTextures[i].wrapMode = TextureWrapMode.Clamp;
            }
        }

        public void SetShaderParams()
        {
            Shader.SetGlobalVector("_VTVtableParam", new Vector4(tableSize, 1.0f / tableSize, 0, 0));
            Shader.SetGlobalVector("_VTTileParam",
                new Vector4(0, 0, PagedTexture.PageCount.x, PagedTexture.PageCount.y));
            Shader.SetGlobalTexture("_VTLookupTex", PageTable.LookupTexture);
            Shader.SetGlobalTexture($"_VTTiledTexArray{0}", PagedTexture.GetTexture());
        }
        private void Update()
        {
            if (enableDebug)
            {
                debuger.RenderDebugPageTableTexture(pageTable, debugPageTableTexture);
                debuger.RenderDebugPagedTexture(_pagedTexture, debugPageTextures);
            }
            SetShaderParams();
        }
        
        public void Load(Vector2Int vPageID)
        {
            Texture2D tex = Resources.Load<Texture>("001_Player_Character_Bag_C") as Texture2D;
            VirtualPage vPage = pageTable.FetchPage(vPageID.x, vPageID.y);
            Vector2Int pPage = new Vector2Int(2, 2);
            vPage.Set(pPage.x, pPage.y);
            _pagedTexture.RenderPage(pPage, tex);
            pageMapping.Add(pPage, vPage);
        }
        void OnGUI()
        {
            if (!enableDebug)
                return;
            if (GUILayout.Button("VT debug", GUILayout.Width(150), GUILayout.Height(50)))
                show = !show;

            if (!show)
                return;
            GUILayout.BeginHorizontal();

            for (int i = 0; i < debugPageTextures.Length; i++)
            {
                GUILayout.Label(debugPageTextures[i], GUILayout.Width(320));
            }
            GUILayout.Label(debugPageTableTexture, GUILayout.Width(320));
            GUILayout.EndHorizontal();
        }
        [ContextMenu("test")]
        void test()
        {
            Vector2Int vPageID = new Vector2Int(1, 3);
            Texture2D tex = Resources.Load<Texture>("001_Player_Character_Bag_C") as Texture2D; 
            VirtualPage vPage = pageTable.FetchPage(vPageID.x, vPageID.y);
            Vector2Int pPage = new Vector2Int(2, 2);
            vPage.Set(pPage.x, pPage.y);
            _pagedTexture.RenderPage(pPage, tex);
            pageMapping.Add(pPage, vPage);
        }
    }
}
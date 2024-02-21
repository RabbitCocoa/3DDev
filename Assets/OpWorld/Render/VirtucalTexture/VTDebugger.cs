using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OpWorld.Render.VT
{
    public class VTDebugger
    {
        private VirtualTexture vt;
        private Material debugMaterial;
        private Shader debugShader = default;

        public VTDebugger(VirtualTexture vt)
        {
            this.vt = vt;
            debugShader = Resources.Load<Shader>("Shaders/VT/VTDebug");
            debugMaterial = new Material(debugShader);
        }

        public void RenderDebugPageTableTexture(PageTable pageTable, RenderTexture debugTexture)
        {
            if (debugTexture == null || pageTable.LookupTexture == null || debugShader == null)
                return;
            debugTexture.DiscardContents();
            Graphics.Blit(pageTable.LookupTexture, debugTexture, debugMaterial, 0);
        }

        public void RenderDebugPagedTexture(PagedTexture pagedTexture, RenderTexture[] debugTextures)
        {
            if (debugTextures == null || debugShader == null)
                return;
            for (int i = 0; i < debugTextures.Length; i++)
            {
                debugTextures[i].DiscardContents();
                Shader.SetGlobalInt("_VTDebugTileParam", i);
                Graphics.Blit(pagedTexture.GetTexture(), debugTextures[i], debugMaterial, 1);
            }
        }
    }
}
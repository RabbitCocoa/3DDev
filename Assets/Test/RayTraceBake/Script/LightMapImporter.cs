using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LightMapImporter : AssetPostprocessor
{
    private void OnPostprocessTexture(Texture2D texture)
    {
        if (assetPath.EndsWith("_LM.png"))
        {
            var tImport = assetImporter as TextureImporter;
            tImport.maxTextureSize = 2048;
            tImport.sRGBTexture = false;
            tImport.mipmapEnabled = false;
            tImport.textureCompression = TextureImporterCompression.Uncompressed;
            tImport.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
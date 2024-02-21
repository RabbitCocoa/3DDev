using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Tool
{
    //菜单
    [MenuItem("Tools/Create Random Color Skybox")]
    static void CreateRandomColorSkybox()
    {
        int size = 512;

        // 创建 Texture2DArray
        Texture2DArray skyboxTextureArray = new Texture2DArray(size, size, 6, TextureFormat.RGB24, false);

        // 为每个面随机填充颜色
        for (int face = 0; face < 6; face++)
        {
            Color[] colors = new Color[size * size];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Random.ColorHSV();
            }

            // 将颜色数组设置到 Texture2DArray 中的相应层
            skyboxTextureArray.SetPixels(colors, face);
        }

        // 应用纹理变化
        skyboxTextureArray.Apply();
        AssetDatabase.CreateAsset(skyboxTextureArray, "Assets/SkyboxTextureArray.asset");
    }
}
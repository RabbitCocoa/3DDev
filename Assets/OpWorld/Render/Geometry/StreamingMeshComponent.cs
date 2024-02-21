using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace OpWorld.Render
{
    public class StreamingMeshComponent : MonoBehaviour
    {
        [SerializeField]
        private StreamingMeshResource meshResource;
        [SerializeField]
        private bool useLOD;
        [SerializeField]
        private int lod;

        public StreamingMeshResource MeshResource { get => meshResource; set => meshResource = value; }

        Material material;

        void Start()
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        void Update()
        {
            if (MeshResource != null)
            {
                if (useLOD)
                {
                    Mesh mesh = MeshResource.GetMesh(lod);
                    if (mesh != null)
                    {
                        Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, 0);
                    }
                }
                else
                {
                    if (MeshResource.Mesh != null)
                    {
                        Graphics.DrawMesh(MeshResource.Mesh, transform.localToWorldMatrix, material, 0);
                    }
                }
            }
        }

        public bool IsLoaded()
        {
            // 从 高LOD到 低LOD 加载
            return MeshResource.IsLoaded(MeshResource.MaxLevel);
        }

        [ContextMenu("test")]
        void Test()
        {
            //StreamingMeshResource mesh = ScriptableObject.CreateInstance<StreamingMeshResource>();
            //string path = Path.Combine(Application.streamingAssetsPath, "model0");
            //mesh.path = path;
            //mesh.modelName = "刷子";
            //mesh.Load();
            //meshResource = mesh;

            //meshResource.Load();
        }

        private void OnDestroy()
        {
            meshResource.Destroy();
        }
    }
}
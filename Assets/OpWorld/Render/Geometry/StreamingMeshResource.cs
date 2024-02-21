using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Render
{
    [Serializable]
    public class StreamingMeshResource 
    {
        public string[] path;
        public string[] modelName;
        private Mesh[] mesh;

        public int MaxLevel => path.Length - 1;

        public bool IsLoaded(int lod) 
        {
            if (mesh == null || mesh[lod] == null)
                return false;
            return true;
        }

        public Mesh Mesh 
        {
            get
            {
                if (IsLoaded(0))
                    return mesh[0];
                else
                {
                    Load(0); // async
                    return null;
                }
            }
        }

        public StreamingMeshResource()
        {
        }

        public void Load(int lod)
        {
            StreamingMeshLoader.SendLoadRequest(new StreamingMeshLoader.LoadRequest()
            {
                path = path[lod],
                modelName = modelName[lod],
                action = (Mesh mesh) =>
                {
                    SetMesh(lod, mesh);
                }
            });
        }

        public void Destroy()
        {
            if (mesh != null)
            {
                for (int i = 0; i < mesh.Length; ++i)
                {
                    mesh[i] = null;
                }
            }
            mesh = null;

            for (int i = 0; i < path.Length; ++i)
            {
                StreamingMeshLoader.SendUnloadRequest(path[i]);
            }
        }


        public Mesh GetMesh(int lod)
        {
            if (IsLoaded(lod))
            {
                return mesh[lod];
            }
            else
            {
                for (int i = lod + 1; mesh != null && i < mesh.Length; ++i)
                {
                    if (mesh[i] != null)
                    {
                        Load(i - 1); // async
                        return mesh[i];
                    }
                }

                lod = this.path.Length - 1; // highest
                Load(lod); // async
                return null;
            }
        }

        private void SetMesh(int lod, Mesh mesh)
        {
            if (this.mesh == null || lod >= this.mesh.Length)
            {
                Array.Resize(ref this.mesh, this.path.Length);
            }
            this.mesh[lod] = mesh;
        }

    }


}
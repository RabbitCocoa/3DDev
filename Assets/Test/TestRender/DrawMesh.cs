using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMesh : MonoBehaviour
{
    public Mesh mesh;

    public Material mat;

    public Camera camera;

    private Matrix4x4 meshMatrix = Matrix4x4.identity;

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        meshMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
        Graphics.DrawMesh(mesh,meshMatrix,mat,0,camera);
    }
    
}
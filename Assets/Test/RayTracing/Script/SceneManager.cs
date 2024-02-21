using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SceneManager : MonoBehaviour
{
    const int maxNumSubMeshes = 32;
    private bool[] subMeshFlagArray = new bool[maxNumSubMeshes];
    private bool[] subMeshCutoffArray = new bool[maxNumSubMeshes];

    private static SceneManager s_Instance;

    public static SceneManager Instance
    {
        get
        {
        
            if (s_Instance != null) return s_Instance;
            s_Instance = GameObject.FindObjectOfType<SceneManager>();
            s_Instance?.Init();
            return s_Instance;
        }
    }

    public Renderer[] renderers;

 
    [System.NonSerialized] public bool isDirty = true;

    public void Awake()
    {
        if (Application.isPlaying)
            DontDestroyOnLoad(this);

        isDirty = true;
    }

    private void Init()
    {
        for (var i = 0; i < maxNumSubMeshes; ++i)
        {
            subMeshFlagArray[i] = true;
            subMeshCutoffArray[i] = false;
        }
    }


    public void FillAccelerationStructure(ref RayTracingAccelerationStructure accelerationStructure)
    {
        foreach (var r in renderers)
        {
            if (r)
                accelerationStructure.AddInstance(r, subMeshFlagArray, subMeshCutoffArray);
        }
    }
}
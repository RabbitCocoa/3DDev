// /*******************
// 文件:CoroutineManagerObject.cs
// 作者:cocoa
// 时间:22:09
// 描述:
// *******************/

using UnityEngine;

namespace OpWorld.Core.ThreadManager
{
    [ExecuteInEditMode]
    public class CoroutineManagerObject : MonoBehaviour
    {
        public int timePerFrame = 3;

        public void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += Update;
#endif
        }

        public void Update()
        {
            CoroutineManager.timePerFrame = timePerFrame;
            CoroutineManager.Update();
        }
    }
}
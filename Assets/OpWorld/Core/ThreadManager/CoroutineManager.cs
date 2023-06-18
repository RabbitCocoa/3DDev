// /*******************
// 文件:CoroutineManager.cs
// 作者:cocoa
// 时间:21:54
// 描述:
// *******************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;

namespace OpWorld.Core.ThreadManager
{
    public static class CoroutineManager
    {
        public class Task
        {
            public IEnumerator routine;
            public string name = null;
            public int priority;

            public void Start()
            {
                CoroutineManager.Enqueue(this);
            }

            public void Stop()
            {
                CoroutineManager.Stop(this);
            }
        }

        private static List<Task> queue = new List<Task>();
        
        private static Task current = null;
        public static float timePerFrame = 1;
        private static Stopwatch timer = new Stopwatch();

        static CoroutineManager()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= AbortOnPlaymodeStateChanged;
            Application.wantsToQuit -= QuitOnExit;
            EditorApplication.playModeStateChanged += AbortOnPlaymodeStateChanged;
            Application.wantsToQuit += QuitOnExit;
#endif
        }
#if UNITY_EDITOR
        static void AbortOnPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
                Quit();
        }
#endif
        public static void Quit()
        {
            queue.Clear();
            if (current != null)
            {
                try
                {
                    if (current.routine != null)
                        current.routine.Reset();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
        }

        static bool QuitOnExit()
        {
            Quit();
            return true;
        }

        public static Task Enqueue(IEnumerator routine, int priority = 0, string name = null)
        {
            Task task = new Task() { routine = routine, priority = priority, name = name };
            Enqueue(task);
            return task;
        }

        public static void Enqueue(Task task)
        {
            queue.Add(task);
        }

        public static void Dequeue(Task task)
        {
        }

        public static void Stop(Task task)
        {
            if (current == task)
                current = null;
            queue.Remove(task);
        }

        public static void Update()
        {
            timer.Reset();
            
            while (timer.ElapsedMilliseconds < timePerFrame || !timer.IsRunning)
            {
                if(!timer.IsRunning) timer.Start();
                if (current == null)
                {
                    if (queue.Count == 0) return;
                    int taskIndex = GetMaxProiorityTaskIndex();

                    current = queue[taskIndex];
                    queue.RemoveAt(taskIndex);
                }

                bool move = false;
                if (current.routine != null)
                    move = current.routine.MoveNext();
                if (!move)
                    current = null;
            }
            
            timer.Stop();

        }

        public static int GetMaxProiorityTaskIndex()
        {
            int maxIndex = -1;
            int maxPriority = int.MinValue;
            for (int i = 0; i < queue.Count; i++)
            {
                if (queue[i].priority > maxPriority)
                {
                    maxIndex = i;
                    maxPriority = queue[i].priority;
                }
            }

            return maxIndex;
        }
    }
}
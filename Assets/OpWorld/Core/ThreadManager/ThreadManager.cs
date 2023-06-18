// /*******************
// 文件:ThreadManager.cs
// 作者:cocoa
// 时间:22:53
// 描述:
// *******************/

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace OpWorld.Core.ThreadManager
{
    public static class ThreadManager
    {
        public class Task
        {
            public Thread thread;
            public Action action;
            public string name = null;
            public int priority;

            public bool Enqueued => waitQueue.Contains(this);
            public bool Working => workQueue.Contains(this);
            public bool IsAlive => Enqueued || Working;
        }
        
        public static bool usemultiThread = false;
        public static int maxThreadCount = 3;
        private static List<Task> waitQueue = new List<Task>();
        private static List<Task> workQueue = new List<Task>();
        public static Task Enqueue(Action action, string name = null, int priority = 0)
        {
            Task task = new Task(){action = action, name = name, priority = priority};
            Enqueue(task);
            return task;
        }

        public static void Enqueue(Task task)
        {
            lock (workQueue)
            {
                if (workQueue.Contains(task))
                    return;
            }

            lock (waitQueue)
            {
                if (waitQueue.Contains(task))
                    return;
                else 
                    waitQueue.Add(task);
            }

            LauchThreads();

        }
        public static void Dequque(Task task){}

        public static void LauchThreads()
        {
            lock (workQueue)
            {

                while (true)
                {
                    if (workQueue.Count >= maxThreadCount)
                        return;

                    Task task;
                    lock (waitQueue)
                    {
                        if (waitQueue.Count == 0) break;
                        int taskIndex = GetMaxProiorityTaskIndex();

                        task = waitQueue[taskIndex];
                        waitQueue.RemoveAt(taskIndex);
                    }
                    workQueue.Add(task);
                    
                    Thread thread = new Thread(task.TaskThreadAction);
                    lock (task)
                        task.thread = thread;
                    thread.Start();
                }
        

            }

        }

        public static void TaskThreadAction(this Task task)
        {
            try
            {
                task.action();
            }
            catch (ThreadAbortException e)
            {
                Debug.LogError(e);
            }
            finally
            {
                lock (workQueue)
                {
                    if (workQueue.Contains(task))
                        workQueue.Remove(task);
                }

                LauchThreads();
            }
        }
        
        public static int GetMaxProiorityTaskIndex()
        {
            int maxIndex = -1;
            int maxPriority = int.MinValue;
            for (int i = 0; i < waitQueue.Count; i++)
            {
                if (waitQueue[i].priority > maxPriority)
                {
                    maxIndex = i;
                    maxPriority = waitQueue[i].priority;
                }
            }

            return maxIndex;
        }
    }
}
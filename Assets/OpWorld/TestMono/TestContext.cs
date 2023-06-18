// /*******************
// 文件:TestContext.cs
// 作者:cocoa
// 时间:22:26
// 描述:
// *******************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using OpWorld.Core.Seralizer;
using OpWorld.Core.ThreadManager;
using OpWorld.Core.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace OpWorld.Tests
{
    public class TestContext : MonoBehaviour
    {
        NativeArray<int> nativeArray;
        int[] array;
        private const int amount = 1000000;
        Stopwatch sw;

        void Start()
        {
            nativeArray = new NativeArray<int>(amount, Allocator.Persistent);
            array = new int[amount];
            // for (int i = 0; i < amount; i++)
            // {
            //     int random = UnityEngine.Random.Range(0,100);
            //     nativeArray[i] = random;
            //     array[i] = random;
            // }
            sw = new Stopwatch();
        }

        private void OnDestroy()
        {
            nativeArray.Dispose();
        }

        [ContextMenu("test")]
        private void test()
        {
            Start();

            int random = UnityEngine.Random.Range(0, 100);
            int read = 0;

            WarmUp(array);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                read = array[i];
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(array)} Read {sw.ElapsedTicks}");
            sw.Reset();

            WarmUp(nativeArray);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                read = nativeArray[i];
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(nativeArray)} Read {sw.ElapsedTicks}");
            sw.Reset();

            WarmUp(array);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                array[i] = random;
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(array)} Write {sw.ElapsedTicks}");
            sw.Reset();

            WarmUp(nativeArray);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                nativeArray[i] = random;
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(nativeArray)} Write {sw.ElapsedTicks}");
            sw.Reset();

            WarmUp(array);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                array[i]++;
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(array)} Read and Write {sw.ElapsedTicks}");
            sw.Reset();

            WarmUp(nativeArray);
            sw.Start();
            for (int i = 0; i < amount; i++)
            {
                nativeArray[i]++;
            }

            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(nativeArray)} RW {sw.ElapsedTicks}");
            sw.Reset();

            var rwJob = new RWJob() { nativeArray = nativeArray };
            rwJob.Schedule().Complete();
            sw.Start();
            rwJob.Schedule().Complete();
            sw.Stop();
            UnityEngine.Debug.Log($"{nameof(nativeArray)} RW (Job) {sw.ElapsedTicks}");
            sw.Reset();

            int[] sizes = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
            foreach (int size in sizes)
            {
                var rwJobParallel = new RWJobParallel() { nativeArray = nativeArray };
                rwJobParallel.Schedule(amount, size).Complete(); //Warm up
                sw.Start();
                rwJobParallel.Schedule(amount, size).Complete();
                sw.Stop();
                UnityEngine.Debug.Log($"{nameof(nativeArray)} RW (Job Parallel {size}) {sw.ElapsedTicks}");
                sw.Reset();
            }
        }

        [BurstCompile]
        struct RWJob : IJob
        {
            public NativeArray<int> nativeArray;

            public void Execute()
            {
                for (int i = 0; i < amount; i++)
                {
                    nativeArray[i]++;
                }
            }
        }

        [BurstCompile]
        struct RWJobParallel : IJobParallelFor
        {
            public NativeArray<int> nativeArray;

            public void Execute(int index)
            {
                nativeArray[index]++;
            }
        }


        private static void WarmUp(int[] a)
        {
            int random = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = random;
            }
        }

        private static void WarmUp(NativeArray<int> a)
        {
            int random = UnityEngine.Random.Range(0, 100);
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = random;
            }
        }
    }
}
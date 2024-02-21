using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using OpWorld.Core;
using OpWorld.Core.Seralizer;
using OpWorld.Core.ThreadManager;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

public class Test1
{
    private CoroutineManager.Task task;

    public class  B
    {
        public A a;
        public int[] arryas;
    }
    public class A
    {
        public int a;
        public List<string> list;
        public C c;
    }

    public class C
    {
        
    }
    
    // A Test behaves as an ordinary method
    [Test]
    public void Test1SimplePasses()
    {
        MemoryManager manager = 
            new MemoryManager(120);
       var s1 =  manager.Allocate(30);
       var s2 =   manager.Allocate(30);
       var s3 =   manager.Allocate(30);
       var s4 =   manager.Allocate(30);
        manager.Free(s2);
       var s5=  manager.Allocate(15);
        var s6 =  manager.Allocate(15);
        manager.Free(s5);
        manager.Free(s6);
        manager.Free(s4);
        manager.Free(s3);
        manager.Allocate(90);
        

    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
   
    
}

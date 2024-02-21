using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OpWorld.Core
{
  // �����ͼ
  // ���ڹ������ݣ����� Ԥ��������
  // ��չʵ�� IComponentBluprint���Զ����Ԥ��������
  // ���� Ԥ�������
  // EditorMode 

  public interface IComponentBlueprint
  {
    public void BuildComponentBlueprint(ComponentBlueprintQueue queue);
    public void CreateInstance(GameObject gameObject);
  }


  // Runtime
  // IComponentBlueprint -> IComponentBlueprintInstance
  public interface IComponentBlueprintInstance
  {
    public void Apply(IComponentBlueprint bp);
  }



  // �������
  public class ComponentBlueprintQueue
  {
    public class Task
    {
      public int Priority { get; set; }
      public Action Action { get; set; }

      public Task(int priority, Action action)
      {
        Priority = priority;
        Action = action;
      }
    }

    public List<Task> TaskQueue = new List<Task>();

    public void Add(int priority, Action action)
    {
      Task task = new Task(priority, action);
      TaskQueue.Add(task);
    }
  }



  }



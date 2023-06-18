using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Core
{
  public abstract class ObjectPool<T>
  {
    private List<T> availableObjects;
    private List<T> usedObjects;

    public ObjectPool(int initialCapacity)
    {
      this.availableObjects = new List<T>(initialCapacity);
      this.usedObjects = new List<T>(initialCapacity);
    }

    public virtual T Get()
    {
      T result = default;

      if (availableObjects.Count == 0)
      {
        result = CreateObjectInstance();
      }
      else
      {
        result = availableObjects[availableObjects.Count - 1];
        availableObjects.RemoveAt(availableObjects.Count - 1);
      }

      usedObjects.Add(result);

      return result;
    }

    public virtual void Return(T instance)
    {
      if (instance == null)
        throw new ArgumentNullException(nameof(instance));
      int indexInUsedObjects = usedObjects.IndexOf(instance);
      if (indexInUsedObjects == -1)
        throw new ArgumentException("Trying to return object that is not part of this pool.", nameof(instance));
      usedObjects.RemoveAt(indexInUsedObjects);
      availableObjects.Add(instance);
    }



    protected abstract T CreateObjectInstance();
    protected abstract void ProcessReturnedInstance(T instance);

  }
}
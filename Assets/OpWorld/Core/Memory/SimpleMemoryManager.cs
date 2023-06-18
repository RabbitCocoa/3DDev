// /*******************
// 文件:SimpleMemoryManager.cs
// 作者:cocoa
// 时间:20:44
// 描述:
// *******************/

using System;
using System.Collections.Generic;

namespace OpWorld.Core
{
    public class SimpleMemoryManager
    {
        public struct SimpleMemorySlot : IComparable<SimpleMemorySlot>, IEquatable<SimpleMemorySlot>
        {
            public uint address; //地址
            public uint size; //大小

            public SimpleMemorySlot(uint size) : this()
            {
                this.address = 0;
                this.size = size;
            }

            public int CompareTo(SimpleMemorySlot other)
            {
                return address.CompareTo(other.address);
            }

            public bool Equals(SimpleMemorySlot other)
            {
                return address == other.address && size == other.size;
            }

            public bool isValid => size > 0;


            public uint end() => address + size;
        }

        public static uint s_1M = 1048576; //1M

        //已经分配的内存碎片
        private LinkedList<SimpleMemorySlot> m_gaps = new LinkedList<SimpleMemorySlot>();
       
        //可用内存块
        private List<SimpleMemorySlot> m_slots = new List<SimpleMemorySlot>();

        private uint m_storage = 0; //存储的总大小
        private uint m_chunkSize = 0; // 每一块的大小

        public void Init(uint storage, uint chunkSize)
        {
            this.m_storage = storage;
            this.m_chunkSize = chunkSize;
        }


        public SimpleMemorySlot Allocate(uint size)
        {
            uint newSize = GetFitSize(size);
            //创建一个内存块
            SimpleMemorySlot newSlot = new SimpleMemorySlot(newSize);
            bool hasReget = false;
            //首先从gap中查找合适的
            for (var i = m_gaps.First; i != null;)
            {
                var next = i.Next;
                SimpleMemorySlot gap = i.Value;
                //如果这个gap的大小足够
                if (gap.size >= newSize)
                {
                    newSlot.address = gap.address;
                    //if (gap.size > newSize) {
                    //  gap.begin += newSize;
                    //  gap.size -= newSize;
                    //}
                    //else {
                    m_gaps.Remove(i);
                    //}
                    hasReget = true;
                    break;
                }

                i = next;
            }
            //如果没有从gap中分配
            if (!hasReget)
            {
                if (m_slots.Count > 0)
                {
                    newSlot.address = m_slots[m_slots.Count - 1].end();
                }   else
                {
                    newSlot.address = 0;
                }
            }
            //如果空间不足
            if (newSlot.isValid && newSlot.end() > m_storage)
            {
                return new SimpleMemorySlot();
            }
            //查找slots中是否存在相同元素 如果不在会返回一个应该插入的位置 这个值是负数
            int insertInd = m_slots.BinarySearch(newSlot);
            if (insertInd < 0)
            {
                m_slots.Insert(~insertInd, newSlot);
            }
            return newSlot;
        }

        public void Free(SimpleMemorySlot slot)
        {
            m_slots.Remove(slot);
            m_gaps.AddLast(slot);
        }
        //返回分配可分配的大小
        private uint GetFitSize(uint size)
        {
            //如果size大于一块 并且还有连续的内存块
            uint c = m_chunkSize;
            while (c < size && c < m_storage)
            {
                c += m_chunkSize;
            }

            return c;
        }
    }
}
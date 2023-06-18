// /*******************
// 文件:MemoryManager.cs
// 作者:cocoa
// 时间:20:02
// 描述:
// *******************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OpWorld.Core
{
    public class MemorySlot
    {
        private int startIndex;
        private int endIndex;
        private bool isGap; //是否为一块空闲区域 且该区域不为末尾 如果是末尾就收缩 而不是创建gap
        private MemorySlot previousSlot = null;
        private MemorySlot nextSlot = null;

        protected MemorySlot(int start, int length, bool isGap)
        {
            this.startIndex = start;
            this.endIndex = start + length;
            this.isGap = isGap;
        }

        public int StartIndex => startIndex;

        public int EndIndex => endIndex;

        public MemorySlot NextSlot => nextSlot;

        public int Length => endIndex - startIndex;

        public MemorySlot PreviousSlot => previousSlot;

        public bool IsGap => isGap;

        //在本快之前插入一个
        public void ConnectToPrevious(MemorySlot previous)
        {
            this.previousSlot = previous;
            if (previous != null)
            {
                previous.nextSlot = this;
            }
        }

        public void ConnectToNext(MemorySlot next)
        {
            this.nextSlot = next;
            if (next != null)
            {
                next.previousSlot = this;
            }
        }

        //内存左移
        public void ShiftLeft(int amount)
        {
            this.startIndex -= amount;
            endIndex -= amount;
        }

        //往后收缩内存
        public void IncreaseStartIndex(int filledLength)
        {
            this.startIndex += filledLength;
        }

        //往前收缩内存
        public void IncreaseEndIndex(int length)
        {
            this.endIndex += length;
        }

        //把自己添加追加到当前链表的末尾 如果currentend不是末尾 那么之后的元素会全部丢失
        public static void Append(MemorySlot newSlot,
            MemorySlot currentEnd)
        {
            if (currentEnd != null)
            {
                currentEnd.ConnectToNext(newSlot);
            }
        }

        //从一个gap内存块中切分一块内存 如果大小和该内存块相同 那么就删除该内存块
        //如果小于 则把当前内存块往后收缩
        //如果大于就有问题了
        public static void InsertInSlot(MemorySlot newSlot,
            MemorySlot slot, List<MemorySlot> slots)
        {
            if (newSlot.Length > slot.Length)
                throw new Exception("切割的内存块大于当前内存块,无法切割");
            if (slot.PreviousSlot != null)
            {
                slot.PreviousSlot.ConnectToNext(newSlot);
            }

            if (slot.Length == newSlot.Length)
            {
                newSlot.ConnectToNext(slot.NextSlot);
                slots.Remove(slot);
            }
            else
            {
                newSlot.ConnectToNext(slot);
                slot.IncreaseStartIndex(newSlot.Length);
            }
        }

        public static MemorySlot CreateGap(int start, int length)
        {
            return new MemorySlot(start, length, true);
        }

        public static MemorySlot CreateSlot(int start, int length)
        {
            MemorySlot slot = new MemorySlot(start, length, false);
            return slot;
        }
    }

    public class MemoryManager
    {
        private int totalLength = 0;
        private int endPointer = 0; //当前内存块的末尾指针地址
        private MemorySlot endSlot = null;

        private List<MemorySlot> gaps = new List<MemorySlot>();

        public MemoryManager(int length)
        {
            totalLength = length;
        }

        public void Dispose()
        {
            gaps.Clear();
        }

        public MemorySlot Allocate(int length)
        {
            MemorySlot newSlot = null;
            foreach (MemorySlot gap in gaps)
            {
                //尝试从gap中切分
                if (gap.Length >= length)
                {
                    newSlot = MemorySlot.CreateSlot(gap.StartIndex, length);
                    MemorySlot.InsertInSlot(newSlot, gap, gaps);
                    return newSlot;
                }
            }

            if ((length + endPointer) > totalLength)
            {
                Debug.LogError("Memory manager Allocate Full!!!");
                return null;
            }

            newSlot = MemorySlot.CreateSlot(endPointer, length);
            MemorySlot.Append(newSlot, endSlot);
            endPointer += length;
            endSlot = newSlot;
            return newSlot;
        }

        public void Free(MemorySlot slot)
        {
            MemorySlot nextSlot = slot.NextSlot;
            MemorySlot previousSlot = slot.PreviousSlot;
            if (nextSlot == null)
            {
                RemoveNextToEnd(slot, nextSlot, previousSlot);
            }
            else if (nextSlot.IsGap)
            {
                RemoveNextToGap(slot, nextSlot, previousSlot);
            }
            else
            {
                RemoveNextToData(slot, nextSlot, previousSlot);
            }
        }

        private void RemoveNextToEnd(MemorySlot slot, MemorySlot nextSlot, MemorySlot previousSlot)
        {
            if (previousSlot != null && previousSlot.IsGap)
            {
                gaps.Remove(previousSlot);

                if (previousSlot.PreviousSlot != null)
                {
                    previousSlot.PreviousSlot.ConnectToNext(null);
                }

                endSlot = previousSlot.PreviousSlot;
                endPointer -= slot.Length + previousSlot.Length;
            }
            else
            {
                if (previousSlot != null)
                {
                    previousSlot.ConnectToNext(null);
                }

                endSlot = previousSlot;
                endPointer -= slot.Length;
            }
        }

        private void RemoveNextToGap(MemorySlot slot, MemorySlot nextSlot, MemorySlot previousSlot)
        {
            if (previousSlot != null && previousSlot.IsGap)
            {
                gaps.Remove(nextSlot);
                previousSlot.ConnectToNext(nextSlot.NextSlot);
                previousSlot.IncreaseEndIndex(slot.Length + nextSlot.Length);
            }
            else
            {
                nextSlot.IncreaseStartIndex(-slot.Length);
                nextSlot.ConnectToPrevious(previousSlot);
            }
        }

        private void RemoveNextToData(MemorySlot slot, MemorySlot nextSlot, MemorySlot previousSlot)
        {
            if (previousSlot != null && previousSlot.IsGap)
            {
                previousSlot.IncreaseEndIndex(slot.Length);
                previousSlot.ConnectToNext(nextSlot);
            }
            else
            {
                MemorySlot gap = MemorySlot.CreateGap(slot.StartIndex, slot.Length);
                gap.ConnectToPrevious(previousSlot);
                gap.ConnectToNext(nextSlot);

                gaps.Add(gap);
            }
        }
    }
}
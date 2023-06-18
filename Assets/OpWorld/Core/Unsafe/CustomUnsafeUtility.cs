// /*******************
// 文件:CustomUnsafeUtility.cs
// 作者:cocoa
// 时间:13:25
// 描述:
// *******************/

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace OpWorld.Core.Unsafe
{
    public unsafe static class CustomUnsafeUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Malloc<T>(long size, Allocator allocator) where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(size, 4, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* MallocAlignOf<T>(long size, int alignment, Allocator allocator) where T : unmanaged
        {
            return (T*)UnsafeUtility.Malloc(size, alignment, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(ref T* ptr, Allocator alloc) where T : unmanaged
        {
            if (ptr != null)
            {
                UnsafeUtility.Free(ptr, alloc);
                ptr = null;
            }
        }

        public static IntPtr ArrayToIntPtr<T>(T[] array) where T : unmanaged
        {
            byte[] bytes = new byte[array.Length * sizeof(T)];
            Buffer.BlockCopy(array, 0, bytes, 0, array.Length);

            int size = bytes.Length;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
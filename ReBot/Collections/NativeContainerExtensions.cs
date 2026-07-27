using System;
using Unity.Collections;

namespace ReBot.Collections;

public static class NativeContainerExtensions
{
    extension<TItem>(NativeArray<TItem>)
        where TItem : unmanaged
    {
        public static NativeArray<TItem> CreateInitialized(int length, Allocator allocator)
        {
            var array = new NativeArray<TItem>(length, allocator, NativeArrayOptions.UninitializedMemory);
            for (int idx = 0; idx < array.Length; idx++)
            {
                array[idx] = new();
            }
            return array;
        }
    }
}

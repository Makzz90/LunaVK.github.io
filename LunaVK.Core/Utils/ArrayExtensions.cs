using System;
using System.Collections.Generic;
using System.Text;

namespace LunaVK.Core.Utils
{
    /// <summary>
    /// Расширения для массивов.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Возвразает указанный диапазон элементов указанного массива.
        /// </summary>
        /// <typeparam name="T">Тип элементов входного массива.</typeparam>
        /// <param name="sourceArray">Исходный массив.</param>
        /// <param name="startIndex">Индекс начала диапазона.</param>
        /// <param name="endIndex">Индекс конца диапазона.</param>
        public static T[] GetRange<T>(this T[] sourceArray, int startIndex, int endIndex)
        {
            if (endIndex < startIndex)
                throw new ArgumentOutOfRangeException("Конечный индекс меньше начального.");

            int count = endIndex - startIndex;
            var result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = sourceArray[i + startIndex];
            return result;
        }
    }
}

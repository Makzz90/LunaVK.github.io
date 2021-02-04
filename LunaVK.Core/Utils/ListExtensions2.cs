using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LunaVK.Core.Utils
{
    public static class ListExtensions2
    {
        public static List<T> Sublist<T>(this List<T> list, int begin, int end)
        {
            List<T> objList = new List<T>();
            for (int index = begin; index < end; ++index)
                objList.Add(list[index]);
            return objList;
        }

        public static void Apply<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T obj in enumerable)
                action(obj);
        }

        public static string GetCommaSeparated(this IReadOnlyList<uint> ids/*, bool invert = false*/)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int count = ids.Count;
            for (int index = 0; index < count; ++index)
            {
                uint to_add = ids[index];//long to_add = invert ? (-ids[index]) : ids[index];
                stringBuilder = stringBuilder.Append(to_add.ToString());
                if (index != count - 1)
                    stringBuilder = stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Cписок чисел, разделенных запятыми
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static string GetCommaSeparated(this IReadOnlyList<int> ids)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int count = ids.Count;
            for (int index = 0; index < count; ++index)
            {
                int to_add = ids[index];
                stringBuilder = stringBuilder.Append(to_add.ToString());
                if (index != count - 1)
                    stringBuilder = stringBuilder.Append(",");
            }
            return stringBuilder.ToString();
        }

        public static string GetCommaSeparated(this IReadOnlyList<string> ids, string separator = ",")
        {
            StringBuilder stringBuilder = new StringBuilder();
            int count = ids.Count;
            for (int index = 0; index < count; ++index)
            {
                stringBuilder = stringBuilder.Append(ids[index]);
                if (index != count - 1)
                    stringBuilder = stringBuilder.Append(separator);
            }
            return stringBuilder.ToString();
        }
        
        //public static bool IsNullOrEmpty(this ICollection list)
        //{
        //    if (list != null)
        //        return list.Count == 0;
        //    return true;
        //}

        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> list)
        {
            if (list != null)
                return list.Count == 0;
            return true;
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            T[] array = (T[])null;
            int newSize = 0;
            foreach (T obj in source)
            {
                if (array == null)
                    array = new T[size];
                array[newSize] = obj;
                ++newSize;
                if (newSize == size)
                {
                    yield return (IEnumerable<T>)new ReadOnlyCollection<T>((IList<T>)array);
                    array = (T[])null;
                    newSize = 0;
                }
            }
            if (array != null)
            {
                Array.Resize<T>(ref array, newSize);
                yield return (IEnumerable<T>)new ReadOnlyCollection<T>((IList<T>)array);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random random = new Random();
            int count = list.Count;
            while (count > 1)
            {
                --count;
                int index = random.Next(count + 1);
                T obj = list[index];
                list[index] = list[count];
                list[count] = obj;
            }
        }
    }
}

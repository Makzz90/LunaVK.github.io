using System;

namespace LunaVK.Core.Framework
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public bool IsPrimaryKey;
        public string Name;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XD.Common
{
    public static class TypeUtil
    {
        public static unsafe long AsLong<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            long value;
            if (sizeof(TEnum) == sizeof(byte))
                value = *(byte*)&enumValue;
            else if (sizeof(TEnum) == sizeof(short))
                value = *(short*)&enumValue;
            else if (sizeof(TEnum) == sizeof(int))
                value = *(int*)&enumValue;
            else if (sizeof(TEnum) == sizeof(long))
                value = *(long*)&enumValue;
            else throw new Exception("type mismatch");
            return value;
        }

        public static bool IsUnmanaged(this Type t)
        {
            bool ret;
            if (CachedTypes.TryGetValue(t, out var isManaged))
                return isManaged;

            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                ret = true;
            else if (t.IsGenericType || !t.IsValueType)
                ret = false;
            else
                ret = t.GetFields(BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Instance)
                    .All(x => x.FieldType.IsUnmanaged());
            CachedTypes.Add(t, ret);
            return ret;
        }
        private static readonly Dictionary<Type, bool> CachedTypes = new();
    }
}
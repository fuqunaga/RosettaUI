using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RosettaUI
{
    // None Generic Interface
    public static class TypeUtility
    {
        class ReflectionCache
        {
            public class MemberData
            {
                public Type type;
                public RangeAttribute range;
            }

            public readonly Dictionary<string, FieldInfo> fieldInfos;
            public readonly Dictionary<string, PropertyInfo> propertyInfos;

            public readonly Dictionary<string, MemberData> memberDatas;
            public readonly List<(string, Type)> serializableFieldsNameAndType;

            public ReflectionCache(IEnumerable<FieldInfo> fis, IEnumerable<PropertyInfo> pis)
            {
                fieldInfos = fis.ToDictionary(fi => fi.Name);
                propertyInfos = pis.ToDictionary(fi => fi.Name);

                memberDatas = fis.Select(fi => (fi as MemberInfo, fi.FieldType))
                    .Concat(pis.Select(pi => (pi as MemberInfo, pi.PropertyType)))
                    .ToDictionary(
                        pair => pair.Item1.Name,
                        pair => new MemberData()
                        {
                            type = pair.Item2,
                            range = pair.Item1.GetCustomAttribute<RangeAttribute>(),
                        }
                    );


                serializableFieldsNameAndType = fis
                    .Where(fi => fi.IsPublic || (fi.GetCustomAttribute<SerializeField>() != null))
                    .Select(fi => (fi.Name, fi.FieldType))
                    .ToList();
            }
        }

        static readonly Dictionary<Type, ReflectionCache> reflectionCache = new Dictionary<Type, ReflectionCache>();


        static ReflectionCache GetReflectionCache(Type type)
        {
            if (!reflectionCache.TryGetValue(type, out var cache))
            {
                cache = new ReflectionCache(
                    type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
                    type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    );

                reflectionCache[type] = cache;
            }

            return cache;
        }

        static ReflectionCache.MemberData GetMemeberData(Type type, string propertyOrFieldName)
        {
            if (!GetReflectionCache(type).memberDatas.TryGetValue(propertyOrFieldName, out var ret))
            {
                Debug.LogError($"{type} does not hava a field or property named [{propertyOrFieldName}].");
            }

            return ret;
        }


        public static Type GetPropertyOrFieldType(Type type, string propertyOrFieldName) => GetMemeberData(type, propertyOrFieldName).type;
        public static RangeAttribute GetRange(Type type, string propertyOrFieldName) => GetMemeberData(type, propertyOrFieldName).range;


        public static bool HasSerializableField(Type type) => GetReflectionCache(type).serializableFieldsNameAndType.Any();
        public static IEnumerable<string> GetSerializableFieldNames(Type type) => GetReflectionCache(type).serializableFieldsNameAndType.Select(pair => pair.Item1);
        public static IEnumerable<Type> GetSerializableFieldTypes(Type type) => GetReflectionCache(type).serializableFieldsNameAndType.Select(pair => pair.Item2);
    }
}

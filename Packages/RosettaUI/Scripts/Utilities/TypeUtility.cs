using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace RosettaUI
{
    public static class TypeUtility
    {
        private static readonly Dictionary<Type, ReflectionCache> ReflectionCacheTable = new Dictionary<Type, ReflectionCache>();

        static TypeUtility()
        {
            RegisterUnityBuiltinProperties();
        }

        static void RegisterUnityBuiltinProperties()
        {
            RegisterUITargetProperties(typeof(Vector2Int), "x", "y");
            RegisterUITargetProperties(typeof(Vector2Int), "x", "y");
            RegisterUITargetProperties(typeof(Vector3Int), "x", "y", "z");
            RegisterUITargetProperties(typeof(Rect),       "x", "y", "width", "height");
            RegisterUITargetProperties(typeof(RectInt),    "x", "y", "width", "height");
            RegisterUITargetProperties(typeof(RectOffset), "left", "right", "top", "bottom");
            RegisterUITargetProperties(typeof(Bounds),     "center", "extents");
            RegisterUITargetProperties(typeof(BoundsInt),  "position", "size");
        }
        
        private static ReflectionCache GetReflectionCache(Type type)
        {
            if (!ReflectionCacheTable.TryGetValue(type, out var cache))
            {
                cache = new ReflectionCache(
                    type,
                    type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance),
                    type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                );

                ReflectionCacheTable[type] = cache;
            }

            return cache;
        }

        private static ReflectionCache.MemberData GetMemberData(Type type, string propertyOrFieldName)
        {
            if (!GetReflectionCache(type).memberDataTable.TryGetValue(propertyOrFieldName, out var ret))
                Debug.LogError($"{type} does not have a field or property named [{propertyOrFieldName}].");

            return ret;
        }


        public static Type GetPropertyOrFieldType(Type type, string propertyOrFieldName)
        {
            return GetMemberData(type, propertyOrFieldName).type;
        }

        public static MemberInfo GetMemberInfo(Type type, string propertyOrFieldName)
        {
            return GetMemberData(type, propertyOrFieldName)?.memberInfo;
        }

        public static RangeAttribute GetRange(Type type, string propertyOrFieldName)
        {
            return GetMemberData(type, propertyOrFieldName).range;
        }


        public static bool HasSerializableField(Type type)
        {
            return GetReflectionCache(type).uiTargetFieldsNameAndType.Any();
        }

        public static IEnumerable<string> GetUITargetFieldNames(Type type)
        {
            return GetReflectionCache(type).uiTargetFieldsNameAndType.Keys;
        }

        public static IEnumerable<Type> GetUITargetFieldTypes(Type type)
        {
            return GetReflectionCache(type).uiTargetFieldsNameAndType.Values;
        }

        public static void RegisterUITargetProperties(Type type, params string[] propertyNames)
        {
            var cache = GetReflectionCache(type);
            foreach (var name in propertyNames)
            {
                if (cache.memberDataTable.TryGetValue(name, out var data))
                {
                    cache.uiTargetFieldsNameAndType[name] = data.type;
                }
            }
        }

        public static void UnregisterUITargetProperties(Type type, string propertyName)
        {
            GetReflectionCache(type).uiTargetFieldsNameAndType.Remove(propertyName);
        }

        public static Type GetListItemType(Type type) => GetReflectionCache(type).listItemType;

        private class ReflectionCache
        {
            public readonly Type listItemType;

            public readonly Dictionary<string, MemberData> memberDataTable;
            public readonly Dictionary<string, Type> uiTargetFieldsNameAndType;

            public ReflectionCache(Type type, IReadOnlyCollection<FieldInfo> fis, IEnumerable<PropertyInfo> pis)
            {
                listItemType = type.GetInterfaces().Concat(new[] {type})
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
                    .Select(t => t.GetGenericArguments().First())
                    .FirstOrDefault();
                
                memberDataTable = fis.Select(fi => (fi as MemberInfo, fi.FieldType))
                    .Concat(pis.Select(pi => (pi as MemberInfo, pi.PropertyType)))
                    .ToDictionary(
                        pair => pair.Item1.Name,
                        pair => new MemberData
                        {
                            type = pair.Item2,
                            memberInfo = pair.Item1,
                            range = pair.Item1.GetCustomAttribute<RangeAttribute>()
                        }
                    );


                uiTargetFieldsNameAndType = fis
                    .Where(fi => fi.IsPublic || fi.GetCustomAttribute<SerializeField>() != null)
                    .ToDictionary(fi => fi.Name, fi => fi.FieldType);
            }

            public class MemberData
            {
                public Type type;
                public MemberInfo memberInfo;
                public RangeAttribute range;
            }
        }
    }
}
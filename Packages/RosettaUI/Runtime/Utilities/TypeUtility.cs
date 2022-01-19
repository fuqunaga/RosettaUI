using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace RosettaUI
{
    public static class TypeUtility
    {
        private static readonly Dictionary<Type, ReflectionCache> ReflectionCacheTable = new();

        static TypeUtility()
        {
            RegisterUnityBuiltinProperties();
        }

        static void RegisterUnityBuiltinProperties()
        {
            RegisterUITargetPropertyOrFields(typeof(Vector2Int), "x", "y");
            RegisterUITargetPropertyOrFields(typeof(Vector2Int), "x", "y");
            RegisterUITargetPropertyOrFields(typeof(Vector3Int), "x", "y", "z");
            RegisterUITargetPropertyOrFields(typeof(Rect),       "x", "y", "width", "height");
            RegisterUITargetPropertyOrFields(typeof(RectInt),    "x", "y", "width", "height");
            RegisterUITargetPropertyOrFields(typeof(RectOffset), "left", "right", "top", "bottom");
            RegisterUITargetPropertyOrFields(typeof(Bounds),     "center", "extents");
            RegisterUITargetPropertyOrFields(typeof(BoundsInt),  "position", "size");
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
            return GetReflectionCache(type).uiTargetPropertyOrFieldsNameTypeDic.Any();
        }

        public static IReadOnlyCollection<string> GetUITargetFieldNames(Type type)
        {
            return GetReflectionCache(type).uiTargetPropertyOrFieldsNameTypeDic.Keys;
        }

        public static IReadOnlyCollection<Type> GetUITargetFieldTypes(Type type)
        {
            return GetReflectionCache(type).uiTargetPropertyOrFieldsNameTypeDic.Values;
        }

        public static void RegisterUITargetPropertyOrFields(Type type, params string[] names)
        {
            var cache = GetReflectionCache(type);
            foreach (var name in names)
            {
                if (cache.memberDataTable.TryGetValue(name, out var data))
                {
                    cache.uiTargetPropertyOrFieldsNameTypeDic[name] = data.type;
                }
            }
        }

        public static void UnregisterUITargetPropertyOrField(Type type, string name)
        {
            GetReflectionCache(type).uiTargetPropertyOrFieldsNameTypeDic.Remove(name);
        }

        public static IEnumerable<string> UnregisterUITargetPropertyOrFieldAll(Type type)
        {
            var cache = GetReflectionCache(type);
            var resistedNames = cache.uiTargetPropertyOrFieldsNameTypeDic.Keys;
            cache.uiTargetPropertyOrFieldsNameTypeDic.Clear();

            return resistedNames;
        }

        public static Type GetListItemType(Type type) => GetReflectionCache(type).listItemType;


        #region MinMax
        
        
        public static readonly ValueTuple<string,string>[] MinMaxMemberNamePairs = {("min", "max"), ("Min", "Max"), ("x", "y")};
        
        public static (string, string) GetMinMaxPropertyOrFieldName(Type type)
        {
            var memberNames = GetReflectionCache(type).memberDataTable.Keys;
            
            return MinMaxMemberNamePairs.FirstOrDefault(pair => memberNames.Contains(pair.Item1) && memberNames.Contains(pair.Item2));
        }
        
        #endregion
        
        
        #region SingleLine
        
        private static readonly Dictionary<Type, bool> SingleLineDic = new();

        public static bool IsSingleLine(Type valueType)
        {
            static bool IsSimpleType(Type t)
            {
                return t.IsPrimitive || t == typeof(string) || t.IsEnum;
            }

            if (UICustom.GetElementCreationMethod(valueType) is { } creationFunc) return creationFunc.isOneLiner;

            if (!SingleLineDic.TryGetValue(valueType, out var ret))
            {
                if (typeof(IElementCreator).IsAssignableFrom(valueType) || typeof(IList).IsAssignableFrom(valueType))
                {
                    ret = false;
                }
                else if (IsSimpleType(valueType))
                {
                    ret = true;
                }
                else
                {
                    const int oneLinerMaxCount = 3;
                    const int nameMaxLength = 3;

                    var fieldTypes = GetUITargetFieldTypes(valueType).ToList();
                    var fieldNames = GetUITargetFieldNames(valueType);

                    ret = fieldTypes.Count <= oneLinerMaxCount
                          && fieldTypes.All(IsSimpleType)
                          && fieldNames.All(name => name.Length <= nameMaxLength
                                                    && GetRange(valueType, name) == null);
                }

                SingleLineDic[valueType] = ret;
            }

            return ret;
        }
        
        #endregion
        
        
        #region Type Define
        
        private class ReflectionCache
        {
            public readonly Type listItemType;

            public readonly Dictionary<string, MemberData> memberDataTable;
            public readonly Dictionary<string, Type> uiTargetPropertyOrFieldsNameTypeDic;

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


                uiTargetPropertyOrFieldsNameTypeDic = fis
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
        
        #endregion


    }
}
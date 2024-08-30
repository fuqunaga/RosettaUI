using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI
{
    public static partial class ClipboardParser
    {
        private static class EditorFormatKey
        {
            public const string Name = "name";
            public const string Type = "type";
            public const string Value = "val";
            public const string ArraySize = "arraySize";
            public const string ArrayType = "arrayType";
            public const string Children = "children";
            public const string ValueNameData = "data";
        }
        
        
        // UnityのClipboardParser.WriteGenericSerializedProperty() 風の機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L385
        public static Dictionary<string, object> ObjectToDictionary(object obj, string fieldName)
        {
            if (obj is null) return null;
            var type = obj.GetType();
            var propertyType = SerializedPropertyTypeRuntimeUtility.TypeToSerializedPropertyType(type); 
            
            var res = new Dictionary<string, object>
            {
                [EditorFormatKey.Name] = fieldName,
                [EditorFormatKey.Type] = (int)propertyType
            };


            var value = propertyType switch
            {
                SerializedPropertyTypeRuntime.Integer => obj,
                SerializedPropertyTypeRuntime.Boolean => obj,
                SerializedPropertyTypeRuntime.Float => obj,
                SerializedPropertyTypeRuntime.String => obj,
                SerializedPropertyTypeRuntime.ArraySize => obj,
                SerializedPropertyTypeRuntime.Character => (int)(char)obj,
                SerializedPropertyTypeRuntime.LayerMask => ((LayerMask)obj).value,
#if UNITY_6000_0_OR_NEWER
                SerializedPropertyTypeRuntime.RenderingLayerMask => ((RenderingLayerMask)obj).value,
#endif
                // SerializedPropertyTypeRuntime.AnimationCurve:
                SerializedPropertyTypeRuntime.Enum => SerializeEnum(obj),
                SerializedPropertyTypeRuntime.Bounds => SerializeBounds((Bounds)obj),
                SerializedPropertyTypeRuntime.Gradient => SerializeGradient((Gradient)obj),
                SerializedPropertyTypeRuntime.Quaternion => SerializeQuaternion((Quaternion)obj),
                SerializedPropertyTypeRuntime.Vector2Int => SerializeVector2((Vector2Int)obj),
                SerializedPropertyTypeRuntime.Vector3Int => SerializeVector3((Vector3Int)obj),
                SerializedPropertyTypeRuntime.RectInt => SerializeRect(ClipboardParserUtility.FromInt((RectInt)obj)),
                SerializedPropertyTypeRuntime.BoundsInt => SerializeBounds(ClipboardParserUtility.FromInt((BoundsInt)obj)),
                
                // Not supported
                // SerializedPropertyTypeRuntime.ObjectReference
                // SerializedPropertyTypeRuntime.ExposedReference
                // SerializedPropertyTypeRuntime.FixedBufferSize
                // SerializedPropertyTypeRuntime.ManagedReference
                _ => null
            };
            
            if ( value != null)
            {
                res[EditorFormatKey.Value] = value;
                return res;
            }


            if (obj is IList list)
            {
                res[EditorFormatKey.ArraySize] = list.Count;
                res[EditorFormatKey.ArrayType] = GetArrayElementTypeName(type);
                res[EditorFormatKey.Children] = new[] { ListToDictionary(list) };

                return res;
            }

            // Supports UITargetFieldNames(include Property)
            // インスペクターでは表示されないプロパティもUI内でやりとり可能にするためサポートする
            var childrenNames = TypeUtility.GetUITargetFieldNames(type);
            if (childrenNames.Any())
            {
                res[EditorFormatKey.Children] = childrenNames.Select(n =>
                {
                    var mi = TypeUtility.GetMemberInfo(type, n);
                    var val = mi switch
                    {
                        FieldInfo fi => fi.GetValue(obj),
                        PropertyInfo pi => pi.GetValue(obj),
                        _ => null
                    };

                    return ObjectToDictionary(val, n);
                }).ToArray();
            }

            return res;
        }

        /// <summary>
        /// インスペクタのシリアライズ準拠
        /// arraySizeを含むDictionaryが入れ子になっている
        /// 
        /// {"name":null,"type":-1,"arraySize":3,"arrayType":"int","children":[
        ///     {"name":"Array","type":-1,"arraySize":3,"arrayType":"int","children":[
        ///         {"name":"size","type":12,"val":3},
        ///         {"name":"data","type":0,"val":1},{"name":"data","type":0,"val":2},{"name":"data","type":0,"val":3}
        ///      ]}
        /// ]}
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static Dictionary<string, object> ListToDictionary(IList list)
        {
            var size = list.Count;
            
            var res = new Dictionary<string, object>
            {
                [EditorFormatKey.Name] = nameof(Array),
                [EditorFormatKey.Type] = (int)SerializedPropertyTypeRuntime.Generic,
                [EditorFormatKey.ArraySize] = size,
                [EditorFormatKey.ArrayType] = GetArrayElementTypeName(list.GetType())
            };

            var children = new object[size + 1];
            children[0] = new Dictionary<string, object>
            {
                [EditorFormatKey.Name] = "size",
                [EditorFormatKey.Type] = (int)SerializedPropertyTypeRuntime.ArraySize,
                [EditorFormatKey.Value] = size
            };
            
            for (var i = 0; i < size; ++i)
            {
                children[i + 1] = ObjectToDictionary(list[i], EditorFormatKey.ValueNameData); 
            }

            res[EditorFormatKey.Children] = children;

            return res;
        }

        
               
        // UnityのClipboardParser.ParseGenericSerializedProperty() 風の機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L452
        public static bool DictionaryToObject(Type type, Dictionary<string, object> dictionary, out object value)
        {
            value = null;
            var hasVal = dictionary.TryGetValue(EditorFormatKey.Value, out var val);

            if (hasVal)
            {
                bool success;
                (success, value) = DictionaryToObject_Primitive(type, val);
                return success;
            }


            // "val" がないタイプ
            var children = TryGetChildren(dictionary);
            if (children is null)
                return false;
            
            return dictionary.ContainsKey(EditorFormatKey.ArraySize)
                ? DictionaryToObject_IList(type, children, out value) 
                : DictionaryToObject_Generic(type, children, out value);
        }

        private static (bool success, object value) DictionaryToObject_Primitive(Type type, object val)
        {
            try
            {
                return type.Name switch
                {
                    nameof(Byte) => ToSuccessTuple(Convert.ToByte(val)),
                    nameof(SByte) => ToSuccessTuple(Convert.ToSByte(val)),
                    nameof(Int16) => ToSuccessTuple(Convert.ToInt16(val)),
                    nameof(UInt16) => ToSuccessTuple(Convert.ToUInt16(val)),
                    nameof(Int32) => ToSuccessTuple(Convert.ToInt32(val)),
                    nameof(UInt32) => ToSuccessTuple(Convert.ToUInt32(val)),
                    nameof(Int64) => ToSuccessTuple(Convert.ToInt64(val)),
                    nameof(UInt64) => ToSuccessTuple(Convert.ToUInt64(val)),
                    nameof(Boolean) => ToSuccessTuple(Convert.ToBoolean(val)),
                    nameof(Single) => ToSuccessTuple(Convert.ToSingle(val)),
                    nameof(Double) => ToSuccessTuple(Convert.ToDouble(val)),
                    nameof(String) => ToSuccessTuple(Convert.ToString(val)),
                    nameof(Char) => ToSuccessTuple(Convert.ToChar(val)),
                    nameof(LayerMask) => ToSuccessTuple(new LayerMask { value = Convert.ToInt32(val) }),
#if UNITY_6000_0_OR_NEWER
                    nameof(RenderingLayerMask) => ToSuccessTuple(new RenderingLayerMask { value =
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  Convert.ToUInt32(val) }),
#endif
                    // nameof(AnimationCurve) => { /* res["val"] = WriteCustom(new AnimationCurveWrapper(p.animationCurveValue)); */ },
                    nameof(Bounds) => ToResultTuple<Bounds>(DeserializeBounds, val),
                    nameof(Gradient) => ToResultTuple<Gradient>(DeserializeGradient, val),
                    nameof(Quaternion) => ToResultTuple<Quaternion>(DeserializeQuaternion, val),
                    nameof(Vector2Int) => ToResultTupleWithConvert<Vector2, Vector2Int>(DeserializeVector2, val,
                        ClipboardParserUtility.ToInt),
                    nameof(Vector3Int) => ToResultTupleWithConvert<Vector3, Vector3Int>(DeserializeVector3, val,
                        ClipboardParserUtility.ToInt),
                    nameof(RectInt) => ToResultTupleWithConvert<Rect, RectInt>(DeserializeRect, val,
                        ClipboardParserUtility.ToInt),
                    nameof(BoundsInt) => ToResultTupleWithConvert<Bounds, BoundsInt>(DeserializeBounds, val,
                        ClipboardParserUtility.ToInt),
                    _ when type.IsEnum => (DeserializeEnum(Convert.ToString(val), type, out var enumValue), enumValue),

                    _ => throw new InvalidCastException()
                };
            }
            catch (Exception)
            {
                // ignored
            }

            return (false, default);


            static (bool, object) ToSuccessTuple<T>(T val) => (true, val);

            static (bool, object) ToResultTuple<T>(DeserializeFunc<T> func, object obj) =>
                (func(Convert.ToString(obj), out var tv), tv);

            static (bool, object) ToResultTupleWithConvert<TBase, TTarget>(DeserializeFunc<TBase> func, object obj,
                Func<TBase, TTarget> convert)
            {
                return func(Convert.ToString(obj), out var baseValue)
                    ? (true, convert(baseValue))
                    : (false, default);
            }
        }

        private static bool DictionaryToObject_IList(Type type, IEnumerable<Dictionary<string, object>> children, out object value)
        {
            value = default;

            if (type.GetInterface(nameof(IList)) == null)
                return false;

            // children の１項目目は name:Array, arraySize, arrayType, children が含まれている
            // data このうちの children
            var firstChild = children.FirstOrDefault();
            if (firstChild is null)
                return false;

            children = TryGetChildren(firstChild);
            

            var elementType = ListUtility.GetItemType(type);

            using var _ = ListPool<object>.Get(out var childrenList);
            childrenList.AddRange(children
                .Where(dic => dic.TryGetValue(EditorFormatKey.Name, out var nameObj) && (nameObj is EditorFormatKey.ValueNameData))
                .Select(dic => (success: DictionaryToObject(elementType, dic, out var v), value: v))
                .Where(t => t.success)
                .Select(t => t.value)
            );
            
            if (childrenList.Count == 0)
                return false;

            IList list;
            if (type.IsArray)
            {
                list = Array.CreateInstance(elementType, childrenList.Count);
                for (var i = 0; i < childrenList.Count; ++i)
                {
                    list[i] = childrenList[i];
                }
            }
            else
            {
                list = (IList)Activator.CreateInstance(type);
                foreach (var child in childrenList)
                {
                    list.Add(child);
                }
            }

            value = list;
            return true;
        }

        private static bool DictionaryToObject_Generic(Type type, IEnumerable<Dictionary<string, object>> children, out object value)
        {
            value = default;

            var childrenNames = TypeUtility.GetUITargetFieldNames(type);
            if (!childrenNames.Any())
                return false;

            var obj = TypeUtility.HasParameterlessConstructor(type)
                ? Activator.CreateInstance(type)
                : RuntimeHelpers.GetUninitializedObject(type);
            
            foreach (var child in children)
            {
                if (!child.TryGetValue(EditorFormatKey.Name, out var nameObject))
                    continue;

                if (nameObject is not string name || string.IsNullOrEmpty(name))
                    continue;

                if (type == typeof(RectOffset))
                {
                    name = RectOffsetMemberNameTable[name];
                }

                var mi = TypeUtility.TryGetMemberInfo(type, name);
                if (mi is null)
                    continue;


                switch (mi)
                {
                    case FieldInfo fi:
                        if (!DictionaryToObject(fi.FieldType, child, out var fv))
                            return false;
                        fi.SetValue(obj, fv);
                        break;

                    case PropertyInfo pi:
                        if (!DictionaryToObject(pi.PropertyType, child, out var pv))
                            return false;
                        var hasSetter = pi.SetMethod != null;
                        if (!hasSetter)
                            return false;
                        
                        pi.SetValue(obj, pv);
                        break;
                }
            }

            value = obj;

            return true;
        }

        private static IEnumerable<Dictionary<string, object>> TryGetChildren(IReadOnlyDictionary<string, object> dictionary)
        {
            if (!dictionary.TryGetValue(EditorFormatKey.Children, out var obj))
                return null;

            return obj is not List<object> list 
                ? null 
                : list.OfType<Dictionary<string, object>>();
        }
        
        
        
        private static string GetArrayElementTypeName(Type type) => TypeNameOrAlias(ListUtility.GetItemType(type));

        private static readonly Dictionary<Type, string> TypeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
        };

        private static string TypeNameOrAlias(Type type) =>
            TypeAlias.TryGetValue(type, out var alias) 
                ? alias 
                : type.Name;

        // RectOffsetのメンバー
        // インスペクターは left -> m_Left などでシリアラズされる
        // おそらく後方互換のためだと思われる
        // leftでもインスペクターへのPasteは反映されるのでそちらに合わせる
        public static readonly Dictionary<string, string> RectOffsetMemberNameTable = new()
        {
            { "m_Left", "left" },
            { "m_Right", "right" },
            { "m_Top", "top" },
            { "m_Bottom", "bottom" },
        };
    }
}
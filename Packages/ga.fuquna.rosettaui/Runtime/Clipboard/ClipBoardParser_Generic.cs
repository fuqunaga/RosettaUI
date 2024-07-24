using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

namespace RosettaUI
{
    public static partial class ClipboardParser
    {
        // UnityのClipboardParser.WriteGenericSerializedProperty() 風の機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L385
        public static Dictionary<string, object> ObjectToDictionary(object obj, string fieldName = null)
        {
            if (obj is null) return null;
            var propertyType = SerializedPropertyTypeRuntimeUtility.TypeToSerializedPropertyType(obj.GetType()); 
            
            var res = new Dictionary<string, object>
            {
                ["name"] = fieldName,
                ["type"] = (int)propertyType
            };


            switch (propertyType)
            {
                case SerializedPropertyTypeRuntime.Integer:
                case SerializedPropertyTypeRuntime.Boolean:
                case SerializedPropertyTypeRuntime.Float:
                case SerializedPropertyTypeRuntime.String:
                case SerializedPropertyTypeRuntime.ArraySize:
                    res["val"] = obj;
                    break;
                
                case SerializedPropertyTypeRuntime.Character:
                    res["val"] = (int)(char)obj;
                    break;

                case SerializedPropertyTypeRuntime.LayerMask:
                    res["val"] = ((LayerMask)obj).value;
                    break;
                
                case SerializedPropertyTypeRuntime.RenderingLayerMask:
                    res["val"] = ((RenderingLayerMask)obj).value;
                    break;
                
                case SerializedPropertyTypeRuntime.AnimationCurve:
                    // res["val"] = WriteCustom(new AnimationCurveWrapper(p.animationCurveValue));
                    break;
                
                case SerializedPropertyTypeRuntime.Enum:
                    res["val"] = SerializeEnum(obj);
                    break;
                case SerializedPropertyTypeRuntime.Bounds:
                    res["val"] = SerializeBounds((Bounds)obj);
                    break;
                case SerializedPropertyTypeRuntime.Gradient:
                    res["val"] = SerializeGradient((Gradient)obj);
                    break;
                case SerializedPropertyTypeRuntime.Quaternion:
                    res["val"] = SerializeQuaternion((Quaternion)obj);
                    break;
                case SerializedPropertyTypeRuntime.Vector2Int:
                    res["val"] = SerializeVector2((Vector2Int)obj);
                    break;
                case SerializedPropertyTypeRuntime.Vector3Int:
                    res["val"] = SerializeVector3((Vector3Int)obj);
                    break;
                case SerializedPropertyTypeRuntime.RectInt:
                    res["val"] = SerializeRect(ClipboardParserUtility.FromInt((RectInt)obj));
                    break;
                case SerializedPropertyTypeRuntime.BoundsInt:
                    var bi = (BoundsInt)obj;
                    res["val"] = SerializeBounds(new Bounds(bi.center, bi.size)); // ClipboardParserUtility.BoundsIntToBounds() とは変換式が異なる
                    break;
                
                // Not supported
                case SerializedPropertyTypeRuntime.ObjectReference: break;
                case SerializedPropertyTypeRuntime.ExposedReference: break;
                case SerializedPropertyTypeRuntime.FixedBufferSize: break;
                case SerializedPropertyTypeRuntime.ManagedReference: break;

                // UnityEditorのClipboardParserでも以下の型はcaseで書かれていないのでそのまま踏襲
                //
                // case SerializedPropertyTypeRuntime.Generic:
                // case SerializedPropertyTypeRuntime.Vector2:
                // case SerializedPropertyTypeRuntime.Vector3:
                // case SerializedPropertyTypeRuntime.Vector4:
                // case SerializedPropertyTypeRuntime.Rect:
                // case SerializedPropertyTypeRuntime.Hash128:
                // case SerializedPropertyTypeRuntime.Color:
                    
                default:
                    var type = obj.GetType();
                    if (obj is IList list)
                    {
                        res["arraySize"] = list.Count;
                        res["arrayType"] = GetArrayElementTypeName(type);
                        res["children"] = new[] { SerializeIList(list) }; // Unityのシリアライズはなぜか配列扱いっぽい
                        
                        return res;
                    }

                    // Supports UITargetFieldNames(include Property)
                    // インスペクターでは表示されないプロパティもUI内でやりとり可能にするためサポートする
                    var childrenNames = TypeUtility.GetUITargetFieldNames(type);
                    if (childrenNames.Any())
                    {
                        res["children"] = childrenNames.Select(n =>
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

                    break;
            }

            return res;
        }

        private static Dictionary<string, object> SerializeIList(IList list)
        {
            var size = list.Count;
            
            var res = new Dictionary<string, object>
            {
                ["name"] = nameof(Array),
                ["type"] = (int)SerializedPropertyTypeRuntime.Generic,
                ["arraySize"] = size,
                ["arrayType"] = GetArrayElementTypeName(list.GetType())
            };

            var children = new object[size + 1];
            children[0] = new Dictionary<string, object>
            {
                ["name"] = "size",
                ["type"] = (int)SerializedPropertyTypeRuntime.ArraySize,
                ["val"] = size
            };
            
            for (var i = 0; i < size; ++i)
            {
                children[i + 1] = ObjectToDictionary(list[i], "data"); 
            }

            res["children"] = children;

            return res;
        }

        private static Type GetArrayElementType(Type type) =>
            type.IsArray
                ? type.GetElementType()
                : type.GetGenericArguments()[0];

        private static string GetArrayElementTypeName(Type type) => TypeNameOrAlias(GetArrayElementType(type));

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
        
        
               
        // UnityのClipboardParser.ParseGenericSerializedProperty() 風の機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L452
        public static bool DictionaryToObject(Type type, Dictionary<string, object> dictionary, out object value)
        {
            value = null;
            dictionary.TryGetValue("val", out var val);
            
            try
            {
                switch (type.Name)
                {
                    case nameof(Int32):
                        value = Convert.ToInt32(val);
                        break;

                    case nameof(UInt32):
                        value = Convert.ToUInt32(val);
                        break;

                    case nameof(Boolean):
                        value = Convert.ToBoolean(val);
                        break;

                    case nameof(Single):
                        value = Convert.ToSingle(val);
                        break;

                    case nameof(String):
                        value = Convert.ToString(val);
                        break;

                    case nameof(Char):
                        value = Convert.ToChar(val);
                        break;

                    case nameof(LayerMask):
                        value = new LayerMask { value = Convert.ToInt32(val) };
                        break;

                    case nameof(RenderingLayerMask):
                        value = new RenderingLayerMask() { value = Convert.ToUInt32(val) };
                        break;


                    case nameof(AnimationCurve):
                        // res["val"] = WriteCustom(new AnimationCurveWrapper(p.animationCurveValue));
                        break;

                    case nameof(Bounds):
                        if (DeserializeBounds(Convert.ToString(val), out var bounds))
                        {
                            value = bounds;
                        }

                        break;

                    case nameof(Gradient):
                        if (DeserializeGradient(Convert.ToString(val), out var gradient))
                        {
                            value = gradient;
                        }

                        break;

                    case nameof(Quaternion):
                        if (DeserializeQuaternion(Convert.ToString(val), out var quaternion))
                        {
                            value = quaternion;
                        }

                        break;

                    case nameof(Vector2Int):
                        if (DeserializeVector2(Convert.ToString(val), out var v2))
                        {
                            value = ClipboardParserUtility.ToInt(v2);
                        }

                        break;

                    case nameof(Vector3Int):
                        if (DeserializeVector3(Convert.ToString(val), out var v3))
                        {
                            value = ClipboardParserUtility.ToInt(v3);
                        }

                        break;

                    case nameof(RectInt):
                        if (DeserializeRect(Convert.ToString(val), out var rect))
                        {
                            value = ClipboardParserUtility.ToInt(rect);
                        }

                        break;

                    case nameof(BoundsInt):
                        if (DeserializeBounds(Convert.ToString(val), out var bi))
                        {
                            value = ClipboardParserUtility.ToInt(bi);
                        }

                        break;

                    default:
                        if (type.IsEnum)
                        {
                            if (DeserializeEnum(Convert.ToString(val), type, out var enumValue))
                            {
                                value = enumValue;
                            }

                            break;
                        }
                        
                        if (!dictionary.TryGetValue("children", out var childrenObj))
                            break;
                        
                        var children = ((List<object>)childrenObj).Cast<Dictionary<string, object>>();


                        // Array/List
                        if (type.GetInterface(nameof(IList)) != null)
                        {
                            var elementType = GetArrayElementType(type);
                            
                            // children は name:Array, arraySize, arrayType, children が含まれている
                            // data このうちの children
                            children = ((List<object>)children.First()["children"]).Cast<Dictionary<string, object>>();
                            Assert.IsNotNull(children);
                            
                            using var _ = ListPool<object>.Get(out var childrenList);
                            childrenList.AddRange(children
                                .Where(dic => dic.TryGetValue("name", out var nameObj) && (nameObj is "data"))
                                .Select(dic => (DictionaryToObject(elementType, dic, out var v), v))
                                .Where(t => t.Item1)
                                .Select(t => t.Item2)
                            );

                            IList list;
                            if (type.IsArray)
                            {
                                list = Array.CreateInstance(elementType, childrenList.Count());
                            }
                            else
                            {
                                list = (IList)Activator.CreateInstance(type);
                            }

                            foreach (var child in childrenList)
                            {
                                list.Add(child);
                            }

                            value = list;
                            
                            break;
                        }

                        // Generic class
                        var childrenNames = TypeUtility.GetUITargetFieldNames(type);
                        if (childrenNames.Any())
                        {
                            var obj = Activator.CreateInstance(type);
                            foreach (var child in children)
                            {
                                if (!child.TryGetValue("name", out var nameObject))
                                    continue;
                                
                                if ( nameObject is not string name || string.IsNullOrEmpty(name))
                                    continue;

                                if (type == typeof(RectOffset))
                                {
                                    name = RectOffsetMemberNameTable[name];
                                }
                                
                                var mi = TypeUtility.GetMemberInfo(type, name);
                                if (mi is null)
                                    continue;
                                

                                switch (mi)
                                {
                                    case FieldInfo fi:
                                        if (!DictionaryToObject(fi.FieldType, child, out var fv))
                                            continue;
                                        fi.SetValue(obj, fv);
                                        break;
                                    
                                    case PropertyInfo pi:
                                        if (!DictionaryToObject(pi.PropertyType, child, out var pv))
                                            continue;
                                        pi.SetValue(obj, pv);
                                        break;
                                }
                            }

                            value = obj;
                        }
                        break;
                }
            }
            catch (InvalidCastException e)
            {
                Debug.LogException(e);
            }

            return true;
        }

    }
}
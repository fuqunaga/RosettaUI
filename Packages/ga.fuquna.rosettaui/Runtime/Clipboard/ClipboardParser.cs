using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace RosettaUI
{
    /// <summary>
    /// Unity inspector compatible parser
    ///
    /// ref: https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/Editor/Mono/Clipboard/ClipboardParser.cs
    /// </summary>
    public static class ClipboardParser
    {
        public const string PrefixEnum = "Enum:";
        public const string PrefixGradient = nameof(UnityEditor) + ".GradientWrapperJSON:";
        public const string PrefixGeneric = "GenericPropertyJSON:";

        private delegate bool DeserializeFunc<T>(string text, out T value);
        private delegate T InitializeWithSpanFunc<out T>(Span<float> values);
                
        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return type.Name switch
            {
                nameof(Boolean) => value.ToString(),
                nameof(Int32) => value.ToString(),
                nameof(UInt32) => value.ToString(),
                nameof(Single) => SerializeFloat(To<float>(ref value)),
                nameof(Vector2) => SerializeVector2(To<Vector2>(ref value)),
                nameof(Vector3) => SerializeVector3(To<Vector3>(ref value)),
                nameof(Vector4) => SerializeVector4(To<Vector4>(ref value)),
                nameof(Vector2Int) => SerializeVector2(To<Vector2Int>(ref value)),
                nameof(Vector3Int) => SerializeVector3(To<Vector3Int>(ref value)),
                nameof(Rect) => SerializeRect(To<Rect>(ref value)),
                nameof(RectInt) => SerializeRect(ClipboardParserUtility.RectIntToRect(To<RectInt>(ref value))),
                nameof(Bounds) => SerializeBounds(To<Bounds>(ref value)),
                nameof(BoundsInt) => SerializeBounds(ClipboardParserUtility.BoundsIntToBounds(To<BoundsInt>(ref value))),
                nameof(Quaternion) => SerializeQuaternion(To<Quaternion>(ref value)),
                nameof(Color) => SerializeColor(To<Color>(ref value)),
                nameof(Gradient) => SerializeGradient(To<Gradient>(ref value)),
                _ when type.IsEnum => SerializeEnum(value),
                _ => SerializeGeneric(in value)
            };

            static TOriginal To<TOriginal>(ref T value) => UnsafeUtility.As<T, TOriginal>(ref value);
        }
        
        
        public static (bool success, T value) Deserialize<T>(string text)
        {
            var type = typeof(T);
            return type.Name switch
            {
                nameof(Boolean) => (DeserializeBool(text, out var v), From(ref v)),
                nameof(Int32) => (DeserializeInt(text, out var v), From(ref v)),
                nameof(UInt32) => (DeserializeUInt(text, out var v), From(ref v)),
                nameof(Single) => (DeserializeFloat(text, out var v), From(ref v)),
                nameof(Vector2) => (DeserializeVector2(text, out var v), From(ref v)),
                nameof(Vector3) => (DeserializeVector3(text, out var v), From(ref v)),
                nameof(Vector4) => (DeserializeVector4(text, out var v), From(ref v)),
                nameof(Vector2Int) => Cast<Vector2, Vector2Int>(text, DeserializeVector2, ClipboardParserUtility.Vector2ToVector2Int),
                nameof(Vector3Int) => Cast<Vector3, Vector3Int>(text, DeserializeVector3, ClipboardParserUtility.Vector3ToVector3Int),
                nameof(Rect) => (DeserializeRect(text, out var v), From(ref v)),
                nameof(RectInt) => Cast<Rect, RectInt>(text, DeserializeRect, ClipboardParserUtility.RectToRectInt),
                nameof(Bounds) => (DeserializeBounds(text, out var v), From(ref v)),
                nameof(BoundsInt) => Cast<Bounds, BoundsInt>(text, DeserializeBounds, ClipboardParserUtility.BoundsToBoundsInt),
                nameof(Quaternion) => (DeserializeQuaternion(text, out var v), From(ref v)),
                nameof(Color) => (DeserializeColor(text, out var v), From(ref v)),
                nameof(Gradient) => (DeserializeGradient(text, out var v), From(ref v)),
                _ when type.IsEnum => (DeserializeEnum<T>(text, out var v), v),
                _ => (false, default)
            };

            static T From<TOriginal>(ref TOriginal value) => UnsafeUtility.As<TOriginal, T>(ref value);

            static (bool, T) Cast<TDeserialize, TTarget>(string text, DeserializeFunc<TDeserialize> deserialize, Func<TDeserialize, TTarget> castFunc)
            {
                var s = deserialize(text, out var v);
                if(!s) return(false, default);

                var value = castFunc(v);
                return (true, From(ref value));
            }
        }

        
        
        public static bool DeserializeBool(string text, out bool value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && bool.TryParse(text, out value);
        }
        
        public static bool DeserializeInt(string text, out int value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && int.TryParse(text, out value);
        }
        
        public static bool DeserializeUInt(string text, out uint value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && uint.TryParse(text, out value);
        }
        
        
        public static string SerializeFloat(float value) => value.ToString(CultureInfo.InvariantCulture);
        
        public static bool DeserializeFloat(string text, out float value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        
        //TODO: supports flag enum
        public static string SerializeEnum(object value)
        {
            var displayName = RuntimeObjectNames.NicifyVariableName(value.ToString());
            return $"{PrefixEnum}{displayName}";
        }
        
        //TODO: supports flag enum
        public static bool DeserializeEnum<TEnum>(string text, out TEnum value)
        {
            value = default;
            if (string.IsNullOrEmpty(text)) return false;
            
            if (!text.StartsWith(PrefixEnum))
                return false;
            
            var val = text[PrefixEnum.Length..];
            if (string.IsNullOrEmpty(val))
                return false;

            var names = Enum.GetNames(typeof(TEnum));
            foreach (var name in names)
            {
                var displayName = RuntimeObjectNames.NicifyVariableName(name);
                if (displayName != text) continue;
                
                value = (TEnum)Enum.Parse(typeof(TEnum), name);
                return true;
            }

            return false;
        }


        public static string SerializeVector2(Vector2 value) => SerializeFloats(nameof(Vector2), value.x, value.y);
        public static string SerializeVector3(Vector3 value) => SerializeFloats(nameof(Vector3),value.x, value.y, value.z);
        public static string SerializeVector4(Vector4 value) => SerializeFloats(nameof(Vector4),value.x, value.y, value.z, value.w);
        public static string SerializeRect(Rect value) => SerializeFloats(nameof(Rect),value.x, value.y, value.width, value.height);
        public static string SerializeQuaternion(Quaternion value) => SerializeFloats(nameof(Quaternion),value.x, value.y, value.z, value.w);
        public static string SerializeBounds(Bounds value) => SerializeFloats(nameof(Bounds), value.center.x, value.center.y, value.center.z, value.extents.x, value.extents.y, value.extents.z);
        
        
        public static bool DeserializeVector2(string text, out Vector2 value)
        {
            return DeserializeFloats(text, 2, out value, values => new Vector2(values[0], values[1]));
        }

        public static bool DeserializeVector3(string text, out Vector3 value)
        {
            return DeserializeFloats(text, 3, out value, values => new Vector3(values[0], values[1], values[2]));
        }

        public static bool DeserializeVector4(string text, out Vector4 value)
        {
            return DeserializeFloats(text, 4, out value, values => new Vector4(values[0], values[1], values[2], values[3]));
        }

        public static bool DeserializeRect(string text, out Rect value)
        {
            return DeserializeFloats(text, 4, out value, values => new Rect(values[0], values[1], values[2], values[3]));
        }
        
        public static bool DeserializeQuaternion(string text, out Quaternion value)
        {
            return DeserializeFloats(text, 4, out value, values => new Quaternion(values[0], values[1], values[2], values[3]), Quaternion.identity);
        }

        public static bool DeserializeBounds(string text, out Bounds value)
        {
            return DeserializeFloats(text, 6, out value, values => new Bounds(
                new Vector3(values[0], values[1], values[2]),
                new Vector3(values[3], values[4], values[5]) * 2f
            ));
        }



        public static string SerializeColor(Color value)
        {
            Color32 ldr = value;
            Color hdrFromLdr = ldr;
            
            if (((Vector4)value - (Vector4)hdrFromLdr).sqrMagnitude < 0.0001f)
            {
                return ldr.a == 255 
                    ? $"#{ColorUtility.ToHtmlStringRGB(value)}"
                    : $"#{ColorUtility.ToHtmlStringRGBA(value)}";
            }

            return SerializeFloats(nameof(Color), value.r, value.g, value.b, value.a);
        }

        public static bool DeserializeColor(string text, out Color value)
        {
            return ColorUtility.TryParseHtmlString(text, out value) 
                   || DeserializeFloats(text, 4, out value, values => new Color(values[0], values[1], values[2], values[3]), Color.black);
        }
        
        
        
        private static string SerializeFloats(string prefix, params float[] values)
        {
            var sb = new StringBuilder();
            sb.Append(prefix);
            sb.Append('(');
            for (var i = 0; i < values.Length; ++i)
            {
                if (i != 0)
                    sb.Append(',');
                sb.Append(values[i].ToString("g9", CultureInfo.InvariantCulture));
            }
            sb.Append(')');
            return sb.ToString();
        }
        
        private static bool DeserializeFloats<T>(string text, int count, out T value, InitializeWithSpanFunc<T> initializeFunc, T? defaultValue = null)
            where T : struct
        {
            Span<float> values = stackalloc float[count];
            var success = DeserializeFloats(text, typeof(T).Name, ref values);
            value = success 
                ? initializeFunc(values)
                : defaultValue ?? default;

            return success;
        }
        
        private static bool DeserializeFloats(string text, string prefix, ref Span<float> values)
        {
            if (string.IsNullOrEmpty(text)) return false;
            
            var count = values.Length;
            
            // build a regex that matches "Prefix(a,b,c,...)" at start of text
            var sb = new StringBuilder();
            sb.Append('^');
            sb.Append(prefix);
            sb.Append("\\(");
            for (var i = 0; i < count; ++i)
            {
                if (i != 0)
                    sb.Append(',');
                sb.Append("([^,]+)");
            }
            sb.Append("\\)");

            var match = Regex.Match(text, sb.ToString());
            if (!match.Success || match.Groups.Count <= count)
                return false;
            
            for (var i = 0; i < count; ++i)
            {
                if (float.TryParse(match.Groups[i + 1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                    values[i] = f;
                else
                    return false;
            }
            
            return true;
        }
  
        
        
        //　Gradient
        // インスペクタはCustomPrefix付きでEditorJsonUtilityでシリアライズしている
        // https://github.com/Unity-Technologies/UnityCsReference/blob/5406f17521a16bb37880960352990229987aa676/Editor/Mono/Clipboard/ClipboardParser.cs#L353
        //
        // さらにGradientWrapperでかぶせている
        public static bool DeserializeGradient(string text, out Gradient gradient)
        {
            gradient = new Gradient();
            if (string.IsNullOrEmpty(text)) return false;


            if (!text.StartsWith(PrefixGradient, StringComparison.OrdinalIgnoreCase)) return false;

            try
            {
                var wrapper = new GradientWrapper();
                JsonUtility.FromJsonOverwrite(text.Remove(0, PrefixGradient.Length), wrapper);
                gradient = wrapper.gradient;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        public static string SerializeGradient(Gradient gradient)
        {
            var wrapper = new GradientWrapper { gradient = gradient };
            return $"{PrefixGradient}{JsonUtility.ToJson(wrapper)}";
        }

        [Serializable]
        private class GradientWrapper
        {
            public Gradient gradient;
        }
        
        
        public static string SerializeGeneric<T>(in T value)
        {
            var dic = ObjectToDictionary(value);
            var json = Json.Serialize(dic);
            return $"{PrefixGeneric}{json}";
        }
        
        // UnityのClipboardParser.WriteGenericSerializedProperty() 風の機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L385
        public static Dictionary<string, object> ObjectToDictionary(object obj, string fieldName = null)
        {
            if (obj is null) return null;
            var propertyType = SerializedPropertyTypeRuntimeUtility.TypeToSerializedPropertyType(obj.GetType()); 
            
            var res = new Dictionary<string, object>()
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
                    res["val"] = SerializeRect(ClipboardParserUtility.RectIntToRect((RectInt)obj));
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
                        res["arrayType"] = GetArrayElementType(type);
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

                            // Unityのシリアラズが独自の名前に変換するもの
                            if (type == typeof(RectOffset))
                            {
                                n = _rectOffsetMemberNameTable[n];
                            }

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
                ["arrayType"] = GetArrayElementType(list.GetType())
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

        private static string GetArrayElementType(Type type)
        {
            var elementType = type.IsArray
                ? type.GetElementType()
                : type.GetGenericArguments()[0];
            
            return TypeNameOrAlias(elementType);
        }

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

        private static Dictionary<string, string> _rectOffsetMemberNameTable = new()
        {
            { "left", "m_Left" },
            { "right", "m_Right" },
            { "top", "m_Top" },
            { "bottom", "m_Bottom" },
        };
    }
}
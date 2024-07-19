using System;
using System.Globalization;
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

        private delegate bool DeserializeFunc<T>(string text, out T value);
        private delegate T InitializeWithSpanFunc<out T>(Span<float> values);
                
        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return Type.GetTypeCode(type) switch
            {
                // TypeCodeは Enum の実態の型なので TypeCode.Int32 などになる。先にEnumの判定を行う
                _ when type.IsEnum => SerializeEnum(value),
                TypeCode.Boolean => value.ToString(),
                TypeCode.Int32 => value.ToString(),
                TypeCode.UInt32 => value.ToString(),
                TypeCode.Single => SerializeFloat(To<float>(ref value)),
                _ when typeof(Vector2) == type => SerializeVector2(To<Vector2>(ref value)),
                _ when typeof(Vector3) == type => SerializeVector3(To<Vector3>(ref value)),
                _ when typeof(Vector4) == type => SerializeVector4(To<Vector4>(ref value)),
                _ when typeof(Vector2Int) == type => SerializeVector2(To<Vector2Int>(ref value)),
                _ when typeof(Vector3Int) == type => SerializeVector3(To<Vector3Int>(ref value)),
                _ when typeof(Rect) == type => SerializeRect(To<Rect>(ref value)),
                _ when typeof(RectInt) == type => SerializeRect(ClipboardParserUtility.RectIntToRect(To<RectInt>(ref value))),
                _ when typeof(Bounds) == type => SerializeBounds(To<Bounds>(ref value)),
                _ when typeof(BoundsInt) == type => SerializeBounds(ClipboardParserUtility.BoundsIntToBounds(To<BoundsInt>(ref value))),
                _ when typeof(Quaternion) == type => SerializeQuaternion(To<Quaternion>(ref value)),
                _ when typeof(Color) == type => SerializeColor(To<Color>(ref value)),
                _ when typeof(Gradient) == type => SerializeGradient(To<Gradient>(ref value)),
                // _ => SerializeGeneric(in value)
            };

            static TOriginal To<TOriginal>(ref T value) => UnsafeUtility.As<T, TOriginal>(ref value);
        }
        
        
        public static (bool success, T value) Deserialize<T>(string text)
        {
            var type = typeof(T);
            return Type.GetTypeCode(type) switch
            {
                // TypeCodeは Enum の実態の型なので TypeCode.Int32 などになる。先にEnumの判定を行う
                _ when type.IsEnum => (DeserializeEnum<T>(text, out var v), v),
                TypeCode.Boolean => (DeserializeBool(text, out var v), From(ref v)),
                TypeCode.Int32 => (DeserializeInt(text, out var v), From(ref v)),
                TypeCode.UInt32 => (DeserializeUInt(text, out var v), From(ref v)),
                TypeCode.Single => (DeserializeFloat(text, out var v), From(ref v)),
                _ when typeof(Vector2) == type => (DeserializeVector2(text, out var v), From(ref v)),
                _ when typeof(Vector3) == type => (DeserializeVector3(text, out var v), From(ref v)),
                _ when typeof(Vector4) == type => (DeserializeVector4(text, out var v), From(ref v)),
                _ when typeof(Vector2Int) == type => Cast<Vector2, Vector2Int>(text, DeserializeVector2, ClipboardParserUtility.Vector2ToVector2Int),
                _ when typeof(Vector3Int) == type => Cast<Vector3, Vector3Int>(text, DeserializeVector3, ClipboardParserUtility.Vector3ToVector3Int),
                _ when typeof(Rect) == type => (DeserializeRect(text, out var v), From(ref v)),
                _ when typeof(RectInt) == type => Cast<Rect, RectInt>(text, DeserializeRect, ClipboardParserUtility.RectToRectInt),
                _ when typeof(Bounds) == type => (DeserializeBounds(text, out var v), From(ref v)),
                _ when typeof(BoundsInt) == type => Cast<Bounds, BoundsInt>(text, DeserializeBounds, ClipboardParserUtility.BoundsToBoundsInt),
                _ when typeof(Quaternion) == type => (DeserializeQuaternion(text, out var v), From(ref v)),
                _ when typeof(Color) == type => (DeserializeColor(text, out var v), From(ref v)),
                _ when typeof(Gradient) == type => (DeserializeGradient(text, out var v), From(ref v)),
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
        
        
        // public static string SerializeGeneric<T>(in T value)
        // {
        //     var json = EditorJsonUtility.ToJson(value);
        //     return json;
        // }

        // ClipboardParser.ParseGenericSerializedProperty() ライクの機能
        // https://github.com/Unity-Technologies/UnityCsReference/blob/77b37cd9f002e27b45be07d6e3667ee53985ec82/Editor/Mono/Clipboard/ClipboardParser.cs#L452
        // 最終的にMiniJsonの出力がインスペクタと揃えばいいので、SerializePropertyType使用せずに簡易化できるかも
#if false
        
        // UnityのClipboardParser.WriteGenericSerializedProperty()を同じ処理を行う
        public static Dictionary<string, object> ObjectToDictionary(object obj, SerializedPropertyTypeRuntime propertyType,  string fieldName = null)
        {
            var res = new Dictionary<string, object>()
            {
                ["name"] = fieldName,
                ["type"] = (int)propertyType
            };


            switch (propertyType)
            {
                case SerializedPropertyTypeRuntime.Integer:
                case SerializedPropertyTypeRuntime.LayerMask:
                case SerializedPropertyTypeRuntime.Character:
                case SerializedPropertyTypeRuntime.RenderingLayerMask:
                case SerializedPropertyTypeRuntime.Boolean:
                case SerializedPropertyTypeRuntime.Float:
                case SerializedPropertyTypeRuntime.String:
                    res["val"] = obj;
                    break;
                
                case SerializedPropertyTypeRuntime.ObjectReference:
                    // res["val"] = WriteCustom(new ObjectWrapper(p.objectReferenceValue));
                    break;
                case SerializedPropertyTypeRuntime.ArraySize:
                    res["val"] = obj;
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

                // Copy/Paste of these for generic serialized properties is not implemented yet.
                case SerializedPropertyTypeRuntime.ExposedReference: break;
                case SerializedPropertyTypeRuntime.FixedBufferSize: break;
                case SerializedPropertyTypeRuntime.ManagedReference: break;

                default:
                    var type = obj.GetType();
                    if (type.IsArray)
                    {
                        res["arraySize"] = ((Array)obj).Length;
                        res["arrayType"] = type.GetElementType();
                    }

                    if (p.hasChildren)
                    {
                        var children = new List<object>();
                        SerializedProperty chit = p.Copy();
                        var end = chit.GetEndProperty();
                        chit.Next(true);
                        while (!SerializedProperty.EqualContents(chit, end))
                        {
                            children.Add(WriteGenericSerializedProperty(chit));
                            if (!chit.Next(false))
                                break;
                        }

                        res["children"] = children;
                    }

                    break;
            }

            return res;
        }
#endif
    }
}
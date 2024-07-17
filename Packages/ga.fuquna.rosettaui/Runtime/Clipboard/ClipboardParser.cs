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
                _ when type.IsEnum                            => SerializeEnum(value),
                TypeCode.Boolean                              => value.ToString(),
                TypeCode.Int32                                => value.ToString(),
                TypeCode.UInt32                               => value.ToString(),
                TypeCode.Single                               => SerializeFloat(To<float>(ref value)),
                _ when typeof(Vector2) == type => SerializeVector2(To<Vector2>(ref value)),
                _ when typeof(Vector3) == type => SerializeVector3(To<Vector3>(ref value)),
                _ when typeof(Vector4) == type => SerializeVector4(To<Vector4>(ref value)),
                _ when typeof(Vector2Int) == type => SerializeVector2(To<Vector2Int>(ref value)),
                _ when typeof(Vector3Int) == type => SerializeVector3(To<Vector3Int>(ref value)),
                _ when typeof(Rect) == type => SerializeRect(To<Rect>(ref value)),
                _ when typeof(Quaternion) == type => SerializeQuaternion(To<Quaternion>(ref value)),
                _ when typeof(Bounds) == type => SerializeBounds(To<Bounds>(ref value)),
                _ when typeof(Gradient) == type => SerializeGradient(To<Gradient>(ref value)),
                _ => ""
            };

            static TOriginal To<TOriginal>(ref T value) => UnsafeUtility.As<T, TOriginal>(ref value);
        }
        
        
        public static (bool success, T value) Deserialize<T>(string text)
        {
            var type = typeof(T);
            return Type.GetTypeCode(type) switch
            {
                // TypeCodeは Enum の実態の型なので TypeCode.Int32 などになる。先にEnumの判定を行う
                _ when type.IsEnum                            => (DeserializeEnum<T> (text, out var v), v),
                TypeCode.Boolean                              => (DeserializeBool    (text, out var v), From(ref v)),
                TypeCode.Int32                                => (DeserializeInt     (text, out var v), From(ref v)),
                TypeCode.UInt32                               => (DeserializeUInt    (text, out var v), From(ref v)),
                TypeCode.Single                               => (DeserializeFloat   (text, out var v), From(ref v)),
                _ when typeof(Vector2) == type => (DeserializeVector2(text, out var v), From(ref v)),
                _ when typeof(Vector3) == type => (DeserializeVector3(text, out var v), From(ref v)),
                _ when typeof(Vector4) == type => (DeserializeVector4(text, out var v), From(ref v)),
                _ when typeof(Vector2Int) == type => Cast<Vector2, Vector2Int>(text, DeserializeVector2, Vector2Int.FloorToInt),
                _ when typeof(Vector3Int) == type => Cast<Vector3, Vector3Int>(text, DeserializeVector3, Vector3Int.FloorToInt),
                _ when typeof(Rect) == type => (DeserializeRect(text, out var v), From(ref v)),
                _ when typeof(Quaternion) == type => (DeserializeQuaternion(text, out var v), From(ref v)),
                _ when typeof(Bounds) == type => (DeserializeBounds(text, out var v), From(ref v)),
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
        public static string SerializeEnum<TEnum>(TEnum value)
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
    }
}
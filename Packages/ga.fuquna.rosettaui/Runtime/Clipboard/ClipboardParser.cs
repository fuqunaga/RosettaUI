using System;
using System.Globalization;
using System.Linq;
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
                _ when typeof(Gradient) == type => (DeserializeGradient(text, out var v), From(ref v)),
                _ => (false, default)
            };

            static T From<TOriginal>(ref TOriginal value) => UnsafeUtility.As<TOriginal, T>(ref value);
        }
        
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
                _ when typeof(Gradient) == type => SerializeGradient(To<Gradient>(ref value)),
                _ => ""
            };

            static TOriginal To<TOriginal>(ref T value) => UnsafeUtility.As<T, TOriginal>(ref value);
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

        
        public static string SerializeVector2(Vector2 value) => string.Format(CultureInfo.InvariantCulture, "Vector2({0:g9},{1:g9})", value.x, value.y);
        
        public static bool DeserializeVector2(string text, out Vector2 value)
        {
            value = default;
            
            Span<float> values = stackalloc float[2];
            if (!DeserializeFloats(text, "Vector2", ref values))
            {
                return false;
            }
            
            value = new Vector2(values[0], values[1]);
            return true;
        }
        
        public static bool DeserializeFloats(string text, string prefix, ref Span<float> values)
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
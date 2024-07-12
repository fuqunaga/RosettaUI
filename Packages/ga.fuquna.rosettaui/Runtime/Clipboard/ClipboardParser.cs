using System;
using System.Globalization;
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
        public static (bool success, T value) Deserialize<T>(string text)
        {
            var type = typeof(T);
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean                              => (DeserializeBool(text, out var v),     From(ref v)),
                TypeCode.Int32                                => (DeserializeInteger(text, out var v),  From(ref v)),
                TypeCode.Single                               => (DeserializeFloat(text, out var v),    From(ref v)),
                TypeCode.Object when typeof(Gradient) == type => (DeserializeGradient(text, out var v), From(ref v)),
                _ => (false, default)
            };

            static T From<TOriginal>(ref TOriginal value) => UnsafeUtility.As<TOriginal, T>(ref value);
        }
        
        public static string Serialize<T>(T value)
        {
            var type = typeof(T);
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean                              => SerializeBool(To<bool>(ref value)),
                TypeCode.Int32                                => SerializeInteger(To<int>(ref value)),
                TypeCode.Single                               => SerializeFloat(To<float>(ref value)),
                TypeCode.Object when typeof(Gradient) == type => SerializeGradient(To<Gradient>(ref value)),
                _ => ""
            };
                
                static TOriginal To<TOriginal>(ref T value) => UnsafeUtility.As<T, TOriginal>(ref value);
        }
        
        
        public static string SerializeBool(bool value) => value.ToString();
        public static bool DeserializeBool(string text, out bool value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && bool.TryParse(text, out value);
        }

        public static string SerializeInteger(int value) => value.ToString();
        public static bool DeserializeInteger(string text, out int value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && int.TryParse(text, out value);
        }

        public static string SerializeFloat(float value) => value.ToString(CultureInfo.InvariantCulture);
        public static bool DeserializeFloat(string text, out float value)
        {
            value = default;
            return !string.IsNullOrEmpty(text) && float.TryParse(text, out value);
        }

        
        public const string PrefixGradient = nameof(UnityEditor) + ".GradientWrapperJSON:";

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
using System;
using System.Collections.Generic;
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
            if (typeof(T) == typeof(bool))
            {
                var success = DeserializeBool(text, out var value);
                return (success, UnsafeUtility.As<bool, T>(ref value));
            }

            if (typeof(T) == typeof(Gradient))
            {
                var success = DeserializeGradient(text, out var value);
                return (success, UnsafeUtility.As<Gradient, T>(ref value));
            }

            return (false, default);
        }
        
        public static string Serialize<T>(T value)
        {
            if (typeof(T) == typeof(bool))
            {
                return SerializeBool(UnsafeUtility.As<T, bool>(ref value));
            }

            if (typeof(T) == typeof(Gradient))
            {
                return SerializeGradient(UnsafeUtility.As<T, Gradient>(ref value));
            }

            return string.Empty;
        }
        
        
        public static bool DeserializeBool(string text, out bool value)
        {
            value = false;
            return !string.IsNullOrEmpty(text) && bool.TryParse(text, out value);
        }

        public static string SerializeBool(bool value) => value.ToString();


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
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace RosettaUI.Test
{
    /// <summary>
    /// Accessor to UnityEditor.ClipboardParser
    ///
    /// ref: https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/Editor/Mono/Clipboard/ClipboardParser.cs
    /// </summary>
    public static class EditorClipBoardParser
    {
        private static readonly Type EditorClipboardParserType = Type.GetType("UnityEditor.ClipboardParser, UnityEditor.CoreModule");
        private static readonly MethodInfo WriteCustomMethodInfo = EditorClipboardParserType.GetMethod("WriteCustom");
        private static readonly MethodInfo ParseCustomMethodInfo = EditorClipboardParserType.GetMethod("ParseCustom");
            
        private static readonly Type GradientWrapperType = Type.GetType("UnityEditor.GradientWrapper, UnityEditor.CoreModule");
        private static readonly FieldInfo GradientWrapperGradientFieldInfo = GradientWrapperType.GetField("gradient");

        public static string WriteBool(bool value) => value.ToString();
        public static (bool, bool) ParseBool(string text) => Parse<bool>(text);
        
        public static string WriteInt(int value) => value.ToString();
        public static (bool, int) ParseInteger(string text) => Parse<int>(text);
        
        public static string WriteUInt(uint value) => value.ToString();
        public static (bool, uint) ParseUint(string text) => Parse<uint>(text);
        
        public static string WriteFloat(float value) => value.ToString(CultureInfo.InvariantCulture);
        public static (bool, float) ParseFloat(string text) => Parse<float>(text);
        
        
        public static string WriteEnum<TEnum>(TEnum value)
        {
            // only support EnumForTest
            Assert.AreEqual(typeof(TEnum), typeof(EnumForTest));
            
            var enumForTestObject = Resources.Load<EnumForTestObject>("EnumForTest");
            enumForTestObject.enumValue = UnsafeUtility.As<TEnum, EnumForTest>(ref value);
            using var so = new SerializedObject(enumForTestObject);
            var prop = so.FindProperty(nameof(EnumForTestObject.enumValue));
            
            var mi = EditorClipboardParserType.GetMethod("WriteEnumProperty");
            Assert.IsNotNull(mi);
            
            var parameters = new object[] { prop };
            return (string)mi.Invoke(null, parameters);
        }
        
        public static (bool, TEnum) ParseEnum<TEnum>(string text)
        {
            // only support EnumForTest
            Assert.AreEqual(typeof(TEnum), typeof(EnumForTest));
            
            if (string.IsNullOrEmpty(text))
            {
                return (false, default);
            }
            
            var enumForTestObject = Resources.Load<EnumForTestObject>("EnumForTest");
            using var so = new SerializedObject(enumForTestObject);
            var prop = so.FindProperty(nameof(EnumForTestObject.enumValue));

            // TODO: support flag enum
            var mi = EditorClipboardParserType.GetMethod("ParseEnumPropertyIndex");
            Assert.IsNotNull(mi);
            var parameters = new object[] { text, prop };
            var idx = (int)mi.Invoke(null, parameters);
            var success = idx >= 0;
            var value = success
                ? (TEnum)Enum.GetValues(typeof(TEnum)).GetValue(idx)
                : default;  
            
            
            return (success, value);
        }

        
        public static string WriteVector2(Vector2 value) => Write(value);
        public static (bool, Vector2) ParseVector2(string text) => Parse<Vector2>(text);

        public static string WriteVector3(Vector3 value) => Write(value);
        public static (bool, Vector3) ParseVector3(string text) => Parse<Vector3>(text);

        public static string WriteVector4(Vector4 value) => Write(value);
        public static (bool, Vector4) ParseVector4(string text) => Parse<Vector4>(text);
        
        
        private static string WriteCustom(object value)
        {
            var methodInfo = WriteCustomMethodInfo.MakeGenericMethod(value.GetType());
            return (string)methodInfo.Invoke(null, new[] {value});
        }

        private static (bool, object) ParseCustom(string text, Type type)
        {
            var methodInfo = ParseCustomMethodInfo.MakeGenericMethod(type);
            var parameters = new object[] {text, null};
            var success = (bool)methodInfo.Invoke(null, parameters);

            return (success, parameters[1]);
        }
            
        public static string WriteGradient(Gradient gradient)
        {
            var gradientWrapper = Activator.CreateInstance(GradientWrapperType, gradient);
            return WriteCustom(gradientWrapper);
        }
            
        public static (bool, Gradient) ParseGradient(string text)
        {
            var (success, gradientWrapper) = ParseCustom(text, GradientWrapperType);
            return (success, (Gradient)GradientWrapperGradientFieldInfo.GetValue(gradientWrapper));
        }

        
        private static string Write<T>(T value)
        {
            var mi = EditorClipboardParserType.GetMethod($"Write{typeof(T).Name}");
            Assert.IsNotNull(mi);
            return (string)mi.Invoke(null, new object[] { value });
        }
        
        private static (bool, T) Parse<T>(string text, [CallerMemberName] string methodName = null)
        {
            Assert.IsFalse(string.IsNullOrEmpty(methodName));
            var mi = EditorClipboardParserType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi);
            var parameters = new object[] { text, null };
            var success = (bool)mi.Invoke(null, parameters);

            return (success, (T)parameters[1]);
        }
    }
}
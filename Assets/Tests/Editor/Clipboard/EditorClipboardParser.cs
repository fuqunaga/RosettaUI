using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        public static string WriteInt(int value) => value.ToString();
        public static string WriteUInt(uint value) => value.ToString();
        public static string WriteFloat(float value) => value.ToString(CultureInfo.InvariantCulture);
        
        public static (bool, bool) ParseBool(string text) => Parse<bool>(text);
        public static (bool, int) ParseInteger(string text) => Parse<int>(text);
        public static (bool, uint) ParseUint(string text) => Parse<uint>(text);
        public static (bool, float) ParseFloat(string text) => Parse<float>(text);
        
        
        
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
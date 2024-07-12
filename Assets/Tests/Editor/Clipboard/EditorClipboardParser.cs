using System;
using System.Reflection;
using UnityEngine;

namespace RosettaUI.Test
{
    /// <summary>
    /// Accessor to UnityEditor.ClipboardParser
    /// </summary>
    public static class EditorClipBoardParser
    {
        private static readonly Type EditorClipboardParserType = Type.GetType("UnityEditor.ClipboardParser, UnityEditor.CoreModule");
        private static readonly MethodInfo WriteCustomMethodInfo = EditorClipboardParserType.GetMethod("WriteCustom");
        private static readonly MethodInfo ParseCustomMethodInfo = EditorClipboardParserType.GetMethod("ParseCustom");
            
        private static readonly Type GradientWrapperType = Type.GetType("UnityEditor.GradientWrapper, UnityEditor.CoreModule");
        private static readonly FieldInfo GradientWrapperGradientFieldInfo = GradientWrapperType.GetField("gradient");

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

        public static string WriteBool(bool value) => value.ToString();
        public static (bool, bool) ParseBool(string text)
        {
            var mi = EditorClipboardParserType.GetMethod("ParseBool");
            var parameters = new object[] { text, null };
            var success = (bool)mi.Invoke(null, parameters);

            return (success, (bool)parameters[1]);
        }
    }
}
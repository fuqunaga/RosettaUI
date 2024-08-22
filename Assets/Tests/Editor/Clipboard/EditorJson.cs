using System;
using System.Reflection;
using UnityEngine.Assertions;

namespace RosettaUI.Test
{
    /// <summary>
    /// Accessor to UnityEditor.Json
    ///
    /// ref: https://github.com/Unity-Technologies/UnityCsReference/blob/6000.0/External/JsonParsers/MiniJson/MiniJSON.cs
    /// </summary>

    public class EditorJson
    {
        private static readonly Type EditorClipboardParserType = Type.GetType("UnityEditor.Json, UnityEditor.CoreModule");
        private static MethodInfo deserializeMethodInfo;
        private static MethodInfo serializeMethodInfo;

        public static object Deserialize(string json)
        {
            deserializeMethodInfo ??= EditorClipboardParserType.GetMethod(nameof(Deserialize));
            Assert.IsNotNull(deserializeMethodInfo);
            
            return deserializeMethodInfo.Invoke(null, new object[] { json });
        }
        
        public static string Serialize(object obj)
        {
            serializeMethodInfo ??= EditorClipboardParserType.GetMethod(nameof(Serialize));
            Assert.IsNotNull(serializeMethodInfo);
            
            return (string)serializeMethodInfo.Invoke(null, new[] { obj, Type.Missing, Type.Missing });
        }
    }
}
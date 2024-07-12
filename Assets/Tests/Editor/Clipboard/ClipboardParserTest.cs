using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace RosettaUI.Test
{
    public class ClipboardParserTest
    {
        [TestCaseSource(nameof(TestSerializeSource))]
        public void TestSerialize<T>(T value, Func<T, string> expectedFunc, Func<T, string> testFunc)
        {
            Assert.AreEqual(expectedFunc(value), testFunc(value));
        }
        
        [TestCaseSource(nameof(TestDeserializeSource))]
        public void TestDeserialize<T>(string text, Func<string, (bool, T)> expectedFunc, Func<string, (bool, T)> testFunc)
        {
            var (expectedSuccess, expectedValue) = expectedFunc(text);
            var (testSuccess, testValue) = testFunc(text);
            Assert.AreEqual(expectedSuccess, testSuccess);
            
            if ( !expectedSuccess ) return;
            if (typeof(T).IsValueType)
            {
                Assert.AreEqual(expectedValue, testValue);
            }
            else
            {
                Assert.AreEqual(JsonUtility.ToJson(expectedValue), JsonUtility.ToJson(testValue));
            }
        }

        
        private static object[] TestSerializeSource()
        {
            return new object[]
            {
                MakeData(new Gradient(), EditorClipBoardParser.WriteGradient, ClipboardParser.SerializeGradient),
                MakeData(true, EditorClipBoardParser.WriteBool, ClipboardParser.SerializeBool),
                MakeData(false, EditorClipBoardParser.WriteBool, ClipboardParser.SerializeBool)
            };
            
            object[] MakeData<T>(T value, Func<T, string> expectedFunc, Func<T, string> testFunc) => new object[] { value, expectedFunc, testFunc };
        }
        
        private static object[] TestDeserializeSource()
        {
            return new object[]
            {
                MakeDeserializeData<Gradient>.MakeData(
                    EditorClipBoardParser.WriteGradient(new Gradient(){mode = GradientMode.Fixed} ), 
                    EditorClipBoardParser.ParseGradient, ClipboardParser.DeserializeGradient)
            };
        }
        
        public static class MakeDeserializeData<T>
        {
            public delegate bool DeserializeFunc(string value, out T result);
            
            public static object[] MakeData(string text, Func<string, (bool, T)>  expectedFunc, DeserializeFunc testFunc)
            {
                return new object[]{text, expectedFunc, DeserializeFuncToFunc(testFunc)};
                
                static Func<string, (bool, T)> DeserializeFuncToFunc(DeserializeFunc deserializeFunc)
                {
                    return value => (deserializeFunc(value, out var result), result);
                }
            }
        }
    }
}
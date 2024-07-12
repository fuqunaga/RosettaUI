using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace RosettaUI.Test
{
    public class ClipboardParserTest
    {
        [TestCaseSource(nameof(TestSerializeSource))]
        public void TestSerialize<T>(T value, Func<T, string> expectedFunc)
        {
            Assert.AreEqual(expectedFunc(value), ClipboardParser.Serialize(value));
        }
        
        [TestCaseSource(nameof(TestDeserializeSource))]
        public void TestDeserialize<T>(string text, Func<string, (bool, T)> expectedFunc)
        {
            var (expectedSuccess, expectedValue) = expectedFunc(text);
            var (testSuccess, testValue) = ClipboardParser.Deserialize<T>(text);
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

        
        private static IEnumerable<object[]> TestSerializeSource()
        {
            var data = new[]
                {
                    MakeData(EditorClipBoardParser.WriteGradient, new Gradient()),
                    MakeData(EditorClipBoardParser.WriteBool, true, false),
                }
                .SelectMany(x => x);

            foreach (var d in data)
            {
                yield return d;
            }
            yield break;
            
            IEnumerable<object[]> MakeData<T>(Func<T, string> expectedFunc, params T[] parameters)
            {
                foreach (var p in parameters)
                {
                    yield return new object[] { p, expectedFunc };
                }
            }
        }
        
        private static IEnumerable<object[]>TestDeserializeSource()
        {
            var data = new[]
                {
                    MakeData(EditorClipBoardParser.ParseGradient, EditorClipBoardParser.WriteGradient(new Gradient() { mode = GradientMode.Fixed })),
                    MakeData(EditorClipBoardParser.ParseBool, "True", "False", "true", "false", "", null),
                }
                .SelectMany(x => x);
            

            foreach (var d in data)
            {
                yield return d;
            }

            yield break;


            IEnumerable<object[]> MakeData<T>(Func<string, (bool, T)> expectedFunc, params string[] texts)
            {
                foreach (var text in texts)
                {
                    yield return new object[] { text, expectedFunc };
                }
            }
        }
    }
}
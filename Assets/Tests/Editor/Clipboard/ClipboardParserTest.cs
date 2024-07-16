using System;
using System.Collections.Generic;
using System.Globalization;
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
                    MakeData(EditorClipBoardParser.WriteInt,  0, 1, 10, -1, -10, int.MinValue, int.MaxValue),
                    MakeData(EditorClipBoardParser.WriteUInt, 0u, 1u, 10u, uint.MinValue, uint.MaxValue),
                    MakeData(EditorClipBoardParser.WriteFloat, 0f, 0.1f, 1f, 10f, -0.1f, -1f, -10f, float.MinValue, float.MaxValue),
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
                    MakeData(EditorClipBoardParser.ParseBool, "True", "False", "true", "false", "", null, "expect parse fail"),
                    MakeData(EditorClipBoardParser.ParseInteger,  "0", "1", "10", "-1", "-10", int.MinValue.ToString(), int.MaxValue.ToString(), "expect parse fail"),
                    MakeData(EditorClipBoardParser.ParseUint, "0", "1", "10", uint.MinValue.ToString(), uint.MaxValue.ToString(), "expect parse fail"),
                    MakeData(EditorClipBoardParser.ParseFloat, "0", "0.1", "1.0", "10", "-0.1", "-1", "-10", float.MinValue.ToString(CultureInfo.InvariantCulture), float.MaxValue.ToString(CultureInfo.InvariantCulture), "expect parse fail"),
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
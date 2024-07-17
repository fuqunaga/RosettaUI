using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace RosettaUI.Test
{
    // ReSharper disable once InconsistentNaming
    public class ClipboardParserTest_Serialize
    {
        [TestCaseSource(nameof(BoolSource))]
        public void MatchUnityEditorMethod_Bool(bool value) => TestMatch(value, EditorClipBoardParser.WriteBool);
        
        [TestCaseSource(nameof(IntSource))]
        public void MatchUnityEditorMethod_Int(int value) => TestMatch(value, EditorClipBoardParser.WriteInt);
        
        [TestCaseSource(nameof(UIntSource))]
        public void MatchUnityEditorMethod_UInt(uint value) => TestMatch(value, EditorClipBoardParser.WriteUInt);
        
        [TestCaseSource(nameof(FloatSource))]
        public void MatchUnityEditorMethod_Float(float value) => TestMatch(value, EditorClipBoardParser.WriteFloat);

        [TestCaseSource(nameof(EnumSource))]
        public void MatchUnityEditorMethod_Enum<TEnum>(TEnum value) => TestMatch(value, EditorClipBoardParser.WriteEnum);

        [TestCaseSource(nameof(Vector2Source))]
        public void MatchUnityEditorMethod_Vector2(Vector2 value) => TestMatch(value, EditorClipBoardParser.WriteVector2);
        
        [TestCaseSource(nameof(GradientSource))]
        public void MatchUnityEditorMethod_Gradient(Gradient value) => TestMatch(value, EditorClipBoardParser.WriteGradient);

        
        private static bool[] BoolSource => new[] {true, false};
        private static int[] IntSource => new[] {0, 1, 10, -1, -10, int.MinValue, int.MaxValue};
        private static uint[] UIntSource => new[] {0u, 1u, 10u, uint.MinValue, uint.MaxValue};
        private static float[] FloatSource => new[] {0f, 0.1f, 1f, 10f, -0.1f, -1f, -10f, float.MinValue, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity, float.Epsilon};
        private static EnumForTest[] EnumSource => Enum.GetValues(typeof(EnumForTest)).Cast<EnumForTest>().ToArray();

        private static Vector2[] Vector2Source => new[]
        {
            Vector2.zero, Vector2.one,
            Vector2.up, Vector2.down, Vector2.left, Vector2.right, 
            Vector2.negativeInfinity, Vector2.positiveInfinity
        };
        
        private static IEnumerable<object> GradientSource()
        {
            yield return new object[] { new Gradient() };
        }

        
        private static void TestMatch<T>(T value, Func<T, string> expectedFunc)
        {
            Assert.AreEqual(expectedFunc(value), ClipboardParser.Serialize(value));
        }
    }
}
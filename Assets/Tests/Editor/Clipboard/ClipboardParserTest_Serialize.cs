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
        
        [TestCaseSource(nameof(Vector3Source))]
        public void MatchUnityEditorMethod_Vector3(Vector3 value) => TestMatch(value, EditorClipBoardParser.WriteVector3);
        
        [TestCaseSource(nameof(Vector4Source))]
        public void MatchUnityEditorMethod_Vector4(Vector4 value) => TestMatch(value, EditorClipBoardParser.WriteVector4);
        
        // UnityEditor.ClipboardParserはVector2Int非対応。ClipboardContextMenuでVector2からキャストしている
        [TestCaseSource(nameof(Vector2IntSource))]
        public void MatchUnityEditorMethod_Vector2Int(Vector2Int value) => TestMatch(value, (v) => EditorClipBoardParser.WriteVector2(v));
        
        // UnityEditor.ClipboardParserはVector3Int非対応。ClipboardContextMenuでVector3からキャストしている
        [TestCaseSource(nameof(Vector3IntSource))]
        public void MatchUnityEditorMethod_Vector3Int(Vector3Int value) => TestMatch(value, (v) => EditorClipBoardParser.WriteVector3(v));
        
        [TestCaseSource(nameof(RectSource))]
        public void MatchUnityEditorMethod_Rect(Rect value) => TestMatch(value, EditorClipBoardParser.WriteRect);

        [TestCaseSource(nameof(QuaternionSource))]
        public void MatchUnityEditorMethod_Quaternion(Quaternion value) => TestMatch(value, EditorClipBoardParser.WriteQuaternion);

        [TestCaseSource(nameof(BoundsSource))]
        public void MatchUnityEditorMethod_Bounds(Bounds value) => TestMatch(value, EditorClipBoardParser.WriteBounds);
        
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
            Vector2.negativeInfinity, Vector2.positiveInfinity,
            Vector2.one * float.Epsilon, Vector2.one * float.NaN, 
            Vector2.one * float.MinValue, Vector2.one * float.MaxValue
        };
        
        private static Vector3[] Vector3Source => new[]
        {
            Vector3.zero, Vector3.one,
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back,
            Vector3.negativeInfinity, Vector3.positiveInfinity,
            Vector3.one * float.Epsilon, Vector3.one * float.NaN, 
            Vector3.one * float.MinValue, Vector3.one * float.MaxValue
        };
        
        private static Vector4[] Vector4Source => new[]
        {
            Vector4.zero, Vector4.one,
            Vector4.negativeInfinity, Vector4.positiveInfinity,
            Vector4.one * float.Epsilon, Vector4.one * float.NaN, 
            Vector4.one * float.MinValue, Vector4.one * float.MaxValue
        };
        
        private static Vector2Int[] Vector2IntSource => new[]
        {
            Vector2Int.zero, Vector2Int.one,
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, 
            Vector2Int.one * int.MinValue, Vector2Int.one * int.MaxValue
        };
        
        private static Vector3Int[] Vector3IntSource => new[]
        {
            Vector3Int.zero, Vector3Int.one,
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right, Vector3Int.forward, Vector3Int.back,  
            Vector3Int.one * int.MinValue, Vector3Int.one * int.MaxValue
        };
        
        private static Rect[] RectSource => new[]
        {
            Rect.zero, new Rect(Vector2.one, Vector2.one),
            new Rect(Vector2.one * float.Epsilon, Vector2.one * float.Epsilon),
            new Rect(Vector2.one * float.NaN, Vector2.one * float.NaN),
            new Rect(Vector2.one * float.NegativeInfinity, Vector2.one * float.NegativeInfinity),
            new Rect(Vector2.one * float.PositiveInfinity, Vector2.one * float.PositiveInfinity),
            new Rect(Vector2.one * float.MinValue, Vector2.one * float.MinValue),
            new Rect(Vector2.one * float.MaxValue, Vector2.one * float.MaxValue),
        };
        
        private static Quaternion[] QuaternionSource => new[]
        {
            default(Quaternion), Quaternion.identity, 
            Quaternion.Euler(90f, 0f, 0f),Quaternion.Euler(0f, 90f, 0f),Quaternion.Euler(0f, 0f, 90f),
            Quaternion.Euler(180f, 0f, 0f),Quaternion.Euler(0f, 180f, 0f),Quaternion.Euler(0f, 0f, 180f),
            Quaternion.Euler(270f, 0f, 0f),Quaternion.Euler(0f, 270f, 0f),Quaternion.Euler(0f, 0f, 270f),
            new Quaternion(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon),
            new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN),
            new Quaternion(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity),
            new Quaternion(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
            new Quaternion(float.MinValue, float.MinValue, float.MinValue, float.MinValue),
            new Quaternion(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue),
        };

        private static IEnumerable<Bounds> BoundsSource => Vector3Source.Select(v3 => new Bounds(v3, v3));
  
        
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
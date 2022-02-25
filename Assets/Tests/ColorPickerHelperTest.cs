using NUnit.Framework;
using RosettaUI.Builder;
using UnityEngine;

namespace RosettaUI.Test
{
    public class ColorPickerHelperTest
    {
        [Test]
        public void Test_CircleToSquare()
        {
            var invSqrt2 = 1f / Mathf.Sqrt(2f);
            //var rad30 = 30.0 * Math.PI / 180.0;

            Assert.AreEqual(Vector2.zero, ColorPickerHelper.CircleToSquare(Vector2.zero));
            
            Assert.AreEqual(new Vector2(1f, 0f), ColorPickerHelper.CircleToSquare(new Vector2(1f, 0f)));
            Assert.AreEqual(Vector2.one, ColorPickerHelper.CircleToSquare(new Vector2(invSqrt2, invSqrt2)));
            Assert.AreEqual(new Vector2(0f, 1f), ColorPickerHelper.CircleToSquare(new Vector2(0f, 1f)));
            Assert.AreEqual(new Vector2(-1f, 1f), ColorPickerHelper.CircleToSquare(new Vector2(-invSqrt2, invSqrt2)));
            Assert.AreEqual(new Vector2(-1f, 0f), ColorPickerHelper.CircleToSquare(new Vector2(-1f, 0f)));
            Assert.AreEqual(new Vector2(-1f, -1f), ColorPickerHelper.CircleToSquare(new Vector2(-invSqrt2, -invSqrt2)));
            Assert.AreEqual(new Vector2(0f, -1f), ColorPickerHelper.CircleToSquare(new Vector2(0f, -1f)));
            Assert.AreEqual(new Vector2(1f, -1f), ColorPickerHelper.CircleToSquare(new Vector2(invSqrt2, -invSqrt2)));
        }
    }
}
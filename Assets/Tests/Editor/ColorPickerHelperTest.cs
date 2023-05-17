using NUnit.Framework;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace RosettaUI.Test
{
    public class ColorPickerHelperTest
    {
        [TestCaseSource(nameof(CircleToSquareSource))]
        public void CircleToSquare(float circleX, float circleY, float squareX, float squareY)
        {
            var comparer = new Vector2EqualityComparer(10e-4f);
            Assert.That(
                ColorPickerHelper.CircleToSquare(new Vector2(circleX, circleY)),
                Is.EqualTo(new Vector2(squareX, squareY)).Using(comparer)
            );
        }

        static object[] CircleToSquareSource()
        {
            var invSqrt2 = 1f / Mathf.Sqrt(2f);
            
            return new object[]
            {
                new []{0f, 0f, 0f, 0f},
                new []{1f, 0f, 1f, 0f},
                new []{invSqrt2, invSqrt2, 1f, 1f},
                new []{0f, 1f, 0f, 1f},
                new []{-invSqrt2, invSqrt2, -1f, 1f},
                new []{-1f, 0f, -1f, 0f},
                new []{-invSqrt2, -invSqrt2, -1f, -1f},
                new []{0f, -1f, 0f, -1f},
                new []{invSqrt2, -invSqrt2, 1f, -1f},
            };
        }
    }
}
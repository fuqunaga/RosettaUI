using NUnit.Framework;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = NUnit.Framework.Is;

namespace RosettaUI.Test
{
    public class GradientHelperTest
    {
        [SetUp]
        public void Setup()
        {
            // 初回呼び出し時にstatic変数を確保でアロケーションするため一度呼んで次回以降アロケーションがないことを確認する
            GradientHelper.EqualsWithoutAlloc(new Gradient(), new Gradient());
        }

        [TestCaseSource(nameof(EqualsWithoutAllocSource))]
        public void EqualsWithoutAlloc(Gradient gradient, Gradient other, bool expected)
        {
            var result = false;

            Assert.That(() =>
            {
                result = GradientHelper.EqualsWithoutAlloc(gradient, other);
            }, Is.Not.AllocatingGCMemory(), "Gradient equality check should not allocate GC memory.");
         
            Assert.That(result, Is.EqualTo(expected), "Gradient equality check failed."); 
        }


        private static object[] EqualsWithoutAllocSource()
        {
            return new object[]
            {
                new object[] { null, null, true },
                new object[] { new Gradient(), null, false },
                new object[] { null, new Gradient(), false },
                new object[] { new Gradient(), new Gradient(), true },
                new object[] { CreateGradient(Color.red, 0.5f), CreateGradient(Color.red, 0.5f), true },
                new object[] { CreateGradient(Color.red, 0.5f), CreateGradient(Color.red, 0.6f), false },
                new object[] { CreateGradient(Color.red, 0.5f), CreateGradient(Color.blue, 0.5f), false },
                new object[] { CreateGradient(Color.red, 0.5f, GradientMode.Fixed), CreateGradient(Color.red, 0.5f, GradientMode.Blend), false },
                new object[] { CreateGradient(Color.red, 0.5f, GradientMode.Fixed, ColorSpace.Linear), CreateGradient(Color.red, 0.5f, GradientMode.Fixed, ColorSpace.Linear), true },
                new object[] { CreateGradient(Color.red, 0.5f, GradientMode.Fixed, ColorSpace.Linear), CreateGradient(Color.red, 0.5f, GradientMode.Fixed, ColorSpace.Gamma), false },
                new object[] { CreateGradient(Color.red, 0.5f, GradientMode.Blend, ColorSpace.Linear), CreateGradient(Color.red, 0.5f, GradientMode.Blend, ColorSpace.Linear), true },
                new object[] { CreateGradient(Color.red, 0.5f, GradientMode.Blend, ColorSpace.Linear), CreateGradient(Color.red, 0.5f, GradientMode.Blend, ColorSpace.Gamma), false },
                
                
            };
        }

        private static Gradient CreateGradient(Color p0, float p1, GradientMode mode = GradientMode.Fixed, ColorSpace colorSpace = ColorSpace.Gamma)
        {
            var gradient = new Gradient
            {
                mode = mode,
                colorSpace = colorSpace
            };
            
            gradient.SetKeys(
                new[] { new GradientColorKey(p0, 0f), new GradientColorKey(p0, 1f) },
                new[] { new GradientAlphaKey(p1, 0f), new GradientAlphaKey(p1, 1f) });
            return gradient;
        }
    }
}
using System.Collections;
using NUnit.Framework;
using RosettaUI.UIToolkit.Builder;

namespace RosettaUI.Test
{
    public class CircularReferenceDetectionTest
    {
        public class MyClass
        {
            public float floatValue; // for Slider
            public MyClass otherMyClass;
        }

        private static MyClass _directRef;
        private static MyClass _farRef;

        static CircularReferenceDetectionTest()
        {
            _directRef = new MyClass();
            _directRef.otherMyClass = _directRef;

            _farRef = new MyClass();
            var other = new MyClass();
            _farRef.otherMyClass = other;
            other.otherMyClass = _farRef;
        }
        

        
        [TestCaseSource(nameof(TestItems))]
        public void Field(MyClass myClass)
        {
            Build(UI.Field(() => myClass));
        }

        [TestCaseSource(nameof(TestItems))]
        public void Slider(MyClass myClass)
        {
            Build(UI.Slider(() => myClass));
        }
        
        [TestCaseSource(nameof(TestItems))]
        public void DynamicElement(MyClass myClass)
        {
            Build(UI.DynamicElementIf(
                () => true,
                () => UI.Field(() => myClass)
                ));
        }

        private static IEnumerable TestItems()
        {
            var data = new[]
            {
                (_directRef, nameof(_directRef)),
                (_farRef, nameof(_farRef)),
            };

            foreach (var (obj, name) in data)
            {
                var caseData = new TestCaseData(obj);
                caseData.SetName(name);
                yield return caseData;
            }
        }
        
        
        void Build(Element element)
        {
            UIToolkitBuilder.Build(element);
        }
    }
}
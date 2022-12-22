using System;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public interface IBuildBindFunc
    {
        VisualElement Build(Element element);
        bool Bind(Element element, VisualElement visualElement);
    }
    
    public class BuildBindFunc<TVisualElement> : IBuildBindFunc
        where TVisualElement : VisualElement, new()
    {
        private readonly Func<Element, VisualElement> _buildFunc;
        private readonly Func<Element, VisualElement, bool> _bindFunc;

        public static BuildBindFunc<TVisualElement> Create(Func<Element, VisualElement, bool> bindFunc) => new(null, bindFunc);

        public BuildBindFunc(Func<Element, VisualElement> buildFunc, Func<Element, VisualElement, bool> bindFunc)
        {
            (_buildFunc, _bindFunc) = (buildFunc, bindFunc);
        }

        public VisualElement Build(Element element) => _buildFunc != null ? _buildFunc(element) : BuildDefault(element);
        public bool Bind(Element element, VisualElement visualElement) => _bindFunc(element, visualElement);

        private VisualElement BuildDefault(Element element)
        {
            var ve = new TVisualElement();
            var success = Bind(element, ve);
            Assert.IsTrue(success);
            return ve;
        }
    }
}
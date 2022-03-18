using System;
using UnityEngine;

namespace RosettaUI.Example
{
    public enum MyEnum
    {
        One,
        Two,
        Three
    }

    [Serializable]
    public class SimpleClass
    {
        public float floatValue;
        public string stringValue;
        private int privateValue; // will be ignored
    }


    [Serializable]
    public class ElementCreator : IElementCreator
    {
        public float floatValue;

        public Element CreateElement() => UI.Slider("By CreteElement()", () => floatValue);
    }

    [Serializable]
    public class UICustomClass
    {
        public float floatValue;
    }


    [Serializable]
    public class ComplexClass
    {
        [Range(0f, 10f)]
        public float floatWithRangeValue; // Fields with a range attribute are automatically displayed with a slider

        public Vector3 vector3Value;
        public SimpleClass simpleClass;
        public ElementCreator elementCreatorClass;
        public UICustomClass uiCustomClass;
    }
}
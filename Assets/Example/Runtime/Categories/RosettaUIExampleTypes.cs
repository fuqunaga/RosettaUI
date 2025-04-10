using System;

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
        public string stringValue;
        public float floatValue;
        private int _privateValue; // will be ignored
    }
}
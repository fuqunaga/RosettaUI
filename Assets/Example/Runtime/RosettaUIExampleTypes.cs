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
        private int _privateValue; // will be ignored
    }
}
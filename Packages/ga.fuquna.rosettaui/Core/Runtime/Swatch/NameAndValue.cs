using System;

namespace RosettaUI.Swatch
{
    [Serializable]
    public struct NameAndValue<TValue>
    {
        public string name;
        public TValue value;
    }
}
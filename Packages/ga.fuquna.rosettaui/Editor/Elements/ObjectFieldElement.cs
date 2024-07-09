using System;
using Object = UnityEngine.Object;

namespace RosettaUI.Editor
{
    public class ObjectFieldElement : FieldBaseElement<Object>
    {
        public readonly Type objectType;
        
        public ObjectFieldElement(LabelElement label, IBinder<Object> binder, Type objectType) : base(label, binder)
        {
            this.objectType = objectType;
        }
    }
}
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class GradientField : UnityInternalAccess.GradientField
    {
        public new class UxmlFactory : UxmlFactory<GradientField, UxmlTraits> { }
    }
}
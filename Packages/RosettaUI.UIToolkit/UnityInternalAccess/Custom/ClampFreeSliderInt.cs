using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public class ClampFreeSliderInt : SliderInt
    {
        public ClampFreeSliderInt()
        {
            clamped = false;
            SliderPatchUtility.CreateTextInputFieldAndBlockSliderKeyDownEvent(this);
        }
        internal override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
            => Mathf.Clamp01(base.SliderNormalizeValue(currentValue, lowerValue, higherValue));

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            
            // TextField.ExecuteDefaultActionAtTarget()を参考にNavigationを殺す
            if (evt != null)
            {
                if (evt.eventTypeId == EventBase<NavigationMoveEvent>.TypeId())
                {
                    evt.StopPropagation();
                    evt.PreventDefault();
                }
            }
        }
    }
}
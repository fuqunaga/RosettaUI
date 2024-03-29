﻿using UnityEngine;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    public interface IValueField<T>
    {
        T value { get; set; }

        void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, T startValue);

        void StartDragging();

        void StopDragging();
    }
}
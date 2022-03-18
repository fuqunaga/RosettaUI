using System;
using System.Collections.Generic;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public class Style : Observable<Style>
    {
        #region Parameters

        float? _width;
        float? _height;
        float? _minWidth;
        float? _minHeight;
        float? _maxWidth;
        float? _maxHeight;
        Color? _color;
        Color? _backgroundColor;

        public float? Width
        {
            get => _width;
            set => SetValue(ref _width, value);
        }
        
        public float? Height
        {
            get => _height;
            set => SetValue(ref _height, value);
        }
        
        public float? MinWidth
        {
            get => _minWidth;
            set => SetValue(ref _minWidth, value);
        }
        public float? MinHeight
        {
            get => _minHeight;
            set => SetValue(ref _minHeight, value);
        }
        
        public float? MaxWidth
        {
            get => _maxWidth;
            set => SetValue(ref _maxWidth, value);
        }
        
        public float? MaxHeight
        {
            get => _maxHeight;
            set => SetValue(ref _maxHeight, value);
        }
        
        public Color? Color
        {
            get => _color;
            set => SetValue(ref _color, value);
        }

        public Color? BackgroundColor
        {
            get => _backgroundColor;
            set => SetValue(ref _backgroundColor, value);
        }
        
        #endregion
        
        void SetValue<T>(ref T current, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(current, newValue))
            {
                current = newValue;
                NotifyValueChanged();
            }
        }

        public override Style GetNotifyValue() => this;
    }
}
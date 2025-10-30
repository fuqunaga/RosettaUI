using System.Collections.Generic;
using RosettaUI.Reactive;
using UnityEngine;

namespace RosettaUI
{
    public class Style : Observable<Style>
    {
        #region Parameters

        private float? _width;
        private float? _height;
        private float? _minWidth;
        private float? _minHeight;
        private float? _maxWidth;
        private float? _maxHeight;
        private Color? _color;
        private Color? _backgroundColor;
        private float? _flexGrow;
        private float? _flexShrink;
        private float? _flexBasis;
        private bool? _flexWrap;

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

        public float? FlexGrow
        {
            get => _flexGrow;
            set => SetValue(ref _flexGrow, value);
        }
        
        public float? FlexShrink
        {
            get => _flexShrink;
            set => SetValue(ref _flexShrink, value);
        }

        public float? FlexBasis
        {
            get => _flexBasis;
            set => SetValue(ref _flexBasis, value);
        }

        public bool? FlexWrap
        {
            get => _flexWrap;
            set => SetValue(ref _flexWrap, value);
        }
        #endregion

        private void SetValue<T>(ref T current, T newValue)
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
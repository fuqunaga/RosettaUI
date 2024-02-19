using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// GradientEditorのキーを編集するクラス
    /// Color/Alphaのキーを編集する
    /// </summary>
    public class GradientKeysEditor
    {
        public static int MaxKeyNum { get; set; } = 8;
        
        private readonly Gradient _gradient;
        private readonly VisualElement _container;
        private readonly List<GradientKeysSwatch> _showedSwatches;
        private readonly bool _isAlpha;
        
        private GradientKeysSwatch _selectedSwatch;
        private readonly Action<GradientKeysSwatch> _onSwatchChanged;
        
        public IReadOnlyList<GradientKeysSwatch> ShowedSwatches => _showedSwatches;
        
        public GradientKeysEditor(Gradient gradient, VisualElement container, List<GradientKeysSwatch> showedSwatches,
            Action<GradientKeysSwatch> onSwatchChanged,
            bool isAlpha
            )
        {
            _gradient = gradient;
            _container = container;
            _showedSwatches = showedSwatches;
            _onSwatchChanged = onSwatchChanged;
            _isAlpha = isAlpha;
            
            foreach(var swatch in _showedSwatches)
            {
                container.Add(swatch.visualElement);
            }
        
            container.RegisterCallback<PointerDownEvent>(OnPointerDownOnContainer);
            PointerDrag.RegisterCallback(container, OnPointerMoveOnContainer);
        }
        
        private void OnPointerDownOnContainer(PointerDownEvent evt)
        {
            var localPos = _container.WorldToLocal(evt.position);
            
            _showedSwatches.Sort((a, b) => a.Time.CompareTo(b.Time));
            var swatch = _showedSwatches.FirstOrDefault(s => s.visualElement.localBound.Contains(localPos));
            
            // 無かったら新規追加
            if (swatch == null)
            {
                swatch = CreateSwatch(localPos.x);
                if (swatch != null)
                {
                    ShowSwatch(swatch);
                }
            }

            // セレクト
            if (swatch != null)
            {
                SelectSwatch(swatch);
                OnSwatchChanged();
            }

            evt.StopPropagation();
        }
        
        private void OnPointerMoveOnContainer(PointerMoveEvent evt)
        {
            if (_selectedSwatch == null) return;
            
            var localPos = _container.WorldToLocal(evt.position);
            var rect = _container.contentRect;
            
            // 左右ではみ出てもカーソル表示はキープ
            if (rect.min.y <= localPos.y && localPos.y <= rect.max.y)
            {
                // なかったら復活
                if (!_showedSwatches.Contains(_selectedSwatch))
                {
                    ShowSwatch(_selectedSwatch);
                }
                
                _selectedSwatch.Time = Mathf.Clamp01(localPos.x / rect.width);
            }
            // 範囲外ではGradient的には削除するがセレクト状態は継続
            // 再度範囲内に戻った場合にColor/Alphaの値が復活する
            else 
            {
                HideSwatch(_selectedSwatch);
            }

            
            OnSwatchChanged();
            evt.StopPropagation();
        }


        private void ShowSwatch(GradientKeysSwatch swatch)
        {
            _showedSwatches.Add(swatch);
            _container.Add(swatch.visualElement);
        }
        
        private void HideSwatch(GradientKeysSwatch swatch)
        {
            _showedSwatches.Remove(swatch);
            swatch.visualElement.RemoveFromHierarchy();
        }
        
        private GradientKeysSwatch CreateSwatch(float localPosX)
        {
            if (_showedSwatches.Count >= MaxKeyNum)
            {
                Debug.LogWarning("Max 8 color keys and 8 alpha keys are allowed in a gradient.");
                return null;
            }

            var t = Mathf.Clamp01(localPosX / _container.resolvedStyle.width);
            var color = _gradient.Evaluate(t);
            
            var newSwatch = new GradientKeysSwatch()
            {
                Time = t
            };
            
            if (_isAlpha)
            {
                newSwatch.Alpha = color.a;
            }
            else
            {
                newSwatch.Color = color;
            }
            
            
            return newSwatch;
        }

        private void SelectSwatch(GradientKeysSwatch swatch)
        {
            UnselectSwatch();

            _selectedSwatch = swatch;
            _selectedSwatch.visualElement.Focus();
            _selectedSwatch.visualElement.BringToFront();
        }
        
        private void UnselectSwatch()
        {
            if (_selectedSwatch == null)
                return;

            _selectedSwatch.visualElement.Blur();
            _selectedSwatch = null;
        }
        
        
        private void OnSwatchChanged()
        {
            _onSwatchChanged?.Invoke(_selectedSwatch);
        }
    }
}
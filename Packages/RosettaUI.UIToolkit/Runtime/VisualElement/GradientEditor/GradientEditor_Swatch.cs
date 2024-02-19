using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public partial class GradientEditor
    {

        private Swatch _selectedSwatch;
        
        private class Swatch
        {
            public float time;
            public Color color;
            public readonly bool isAlpha;
            public VisualElement cursor;

            public Swatch(float time, Color value, bool isAlpha, VisualElement cursor)
            {
                this.time = time;
                color = value;
                this.isAlpha = isAlpha;
                this.cursor = cursor;
            }
        }
        
        
        private static Swatch CreateSwatch(float time, Color color, bool isAlpha)
        {
            var cursor = CreateCursor(isAlpha);
            cursor.style.unityBackgroundImageTintColor = color;
            var swatch = new Swatch(time, color, isAlpha, cursor);
            return swatch;
        }

        private static VisualElement CreateCursor(bool isAlpha = false)
        {
            var cursor = new VisualElement();
            cursor.AddToClassList(CursorUSSClassName);
            cursor.AddToClassList(isAlpha
                ? AlphaCursorUSSClassName
                : ColorCursorUSSClassName);

            cursor.focusable = true;
            return cursor;
        }

        private void InitCursorContainer(VisualElement container)
        {
            container.RegisterCallback<PointerDownEvent>(OnPointerDownOnAlphaContainer);
            PointerDrag.RegisterCallback(container, OnPointerMoveOnAlphaContainer);
        }
        
        
        #region Alpha Cursors Events

        private void OnPointerDownOnAlphaContainer(PointerDownEvent evt)
        {
            // var localPos = _alphaCursorContainer.WorldToLocal(evt.position);
            //
            // var swatch = ContainsSwatchAtPosition(localPos, _alphaSwatches, true);
            // if (swatch != null)
            // {
            //     SelectSwatch(swatch);
            // }
            // else
            // {
            //     // 無かったら新規追加
            //     AddAndSelectSwatch(localPos.x, _alphaSwatches, _alphaCursorContainer);
            // }
            //
            // evt.StopPropagation();
        }

        private void OnPointerMoveOnAlphaContainer(PointerMoveEvent evt)
        {
            var localPos = _alphaCursorContainer.WorldToLocal(evt.position);
            var rect = _alphaCursorContainer.contentRect;
            
            // 左右ではみ出てもカーソル表示はキープ
            if (rect.min.y <= localPos.y && localPos.y <= rect.max.y)
            {
                if (_selectedSwatch == null)
                {
                    AddAndSelectSwatch(localPos.x, _alphaSwatches, _alphaCursorContainer);
                }
                else
                {
                    _selectedSwatch.time = Mathf.Clamp01(LocalPosToTime(_alphaCursorContainer, localPos.x));
                    if (!_alphaSwatches.Contains(_selectedSwatch))
                    {
                        AddSwatch(_selectedSwatch, _alphaSwatches, _alphaCursorContainer);
                    }
                    UpdateSelectedSwatchField();
                    UpdateSwatches(_alphaSwatches, _alphaCursorContainer);
                    OnGradientChanged();
                }
            }
            // 範囲外ではGradient的には削除するがセレクト状態は継続
            // 再度範囲内に戻った場合にColor/Alphaの値が復活する
            else if (_selectedSwatch != null)
            {
                RemoveSwatch(_selectedSwatch, _alphaSwatches, _alphaCursorContainer);
                OnGradientChanged();
            }

            evt.StopPropagation();
        }
        
        private void AddSwatch(Swatch swatch, List<Swatch> swatches, VisualElement container)
        {
            swatches.Add(swatch);
            container.Add(swatch.cursor);
            UpdateSwatches(swatches, container);
        }
        
        private void AddAndSelectSwatch(float localPosX, List<Swatch> swatches, VisualElement container)
        {
            if (swatches.Count < MaxKeyNum)
            {
                var t = Mathf.Clamp01(LocalPosToTime(container, localPosX));
                var a = _gradient.Evaluate(t).a;
                var newSwatch = CreateSwatch(t, new Color(a, a, a, 1), true);
                AddSwatch(newSwatch, swatches, container);
                SelectSwatch(newSwatch);
            }
            else
            {
                Debug.LogWarning("Max 8 color keys and 8 alpha keys are allowed in a gradient.");
            }
        }

        #endregion
        
    }
}
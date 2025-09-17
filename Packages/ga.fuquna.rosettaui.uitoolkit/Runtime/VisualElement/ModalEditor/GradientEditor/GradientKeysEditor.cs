﻿using System;
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
        
        private readonly VisualElement _container;
        private readonly List<GradientKeysSwatch> _showedSwatches = new();
        private readonly bool _isAlpha;
        
        private Gradient _gradient;
        private GradientKeysSwatch _selectedSwatch;
        private readonly Action<GradientKeysSwatch> _onAddSwatch;
        private readonly Action<GradientKeysSwatch> _onRemoveSwatch;
        private readonly Action<GradientKeysSwatch> _onSelectedSwatchChanged;
        private readonly Action<GradientKeysSwatch> _onSwatchValueChanged;
        
        public IReadOnlyList<GradientKeysSwatch> ShowedSwatches => _showedSwatches;
        
        public GradientKeysEditor(VisualElement container, 
            Action<GradientKeysSwatch> onAddSwatch,
            Action<GradientKeysSwatch> onRemoveSwatch,
            Action<GradientKeysSwatch> onSelectedSwatchChanged,
            Action<GradientKeysSwatch> onSwatchValueChanged,
            bool isAlpha
            )
        {
            _container = container;
            _onAddSwatch = onAddSwatch;
            _onRemoveSwatch = onRemoveSwatch;
            _onSelectedSwatchChanged = onSelectedSwatchChanged;
            _onSwatchValueChanged = onSwatchValueChanged;
            _isAlpha = isAlpha;
            
            container.RegisterCallback<KeyDownEvent>(OnKeyDownOnContainer);
            container.AddManipulator(new DragManipulator(OnDragStart, OnDrag, OnDragEnd));
        }

        public void Initialize(Gradient gradient, IEnumerable<GradientKeysSwatch> showedSwatches)
        {
            _gradient = gradient;
            
            _container.Clear();
            
            _showedSwatches.Clear();
            _showedSwatches.AddRange(showedSwatches);
            
            foreach(var swatch in _showedSwatches)
            {
                _container.Add(swatch.visualElement);
            }
        }
        

        private bool OnDragStart(PointerDownEvent evt)
        {
            var localPos = _container.WorldToLocal(evt.position);
            
            _showedSwatches.Sort((a, b) => a.TimePercent.CompareTo(b.TimePercent));
            var swatch = _showedSwatches.FirstOrDefault(s => s.visualElement.localBound.Contains(localPos));
            
            // 無かったら新規追加
            if (swatch == null)
            {
                swatch = CreateSwatch(localPos.x);
                if (swatch != null)
                {
                    AddSwatch(swatch);
                    swatch.visualElement.schedule.Execute(() =>
                    {
                        // 追加直後にフォーカスを当てる
                        SelectSwatch(swatch);
                    });
                }
            }
            // セレクト
            else
            {
                SelectSwatch(swatch);
            }

            evt.StopPropagation();
            return true;
        }
        
        private void OnDrag(PointerMoveEvent evt)
        {
            if (_selectedSwatch == null) return;
            
            var localPos = _container.WorldToLocal(evt.position);
            var rect = _container.contentRect;
            
            var updateTime = false;
            
            // 左右ではみ出てもカーソル表示はキープ
            if (rect.min.y <= localPos.y && localPos.y <= rect.max.y)
            {
                // なかったら復活
                if (!_showedSwatches.Contains(_selectedSwatch))
                {
                    AttachSwatch(_selectedSwatch);
                    SelectSwatch(_selectedSwatch);
                }
                
                updateTime = true;   
            }
            // 範囲外ではGradient的には削除するがセレクト状態は継続
            // 再度範囲内に戻った場合にColor/Alphaの値が復活する
            // ただし、1つしかない場合は削除せずTimeを更新する
            else 
            {
                if ( _showedSwatches.Count > 1)
                {
                    DetachSwatch(_selectedSwatch);
                }
                else
                {
                    updateTime = true;
                }
            }

            if (updateTime)
            {
                _selectedSwatch.Time = Mathf.Clamp01(localPos.x / rect.width);
            }

            OnSwatchValueChanged();
            evt.StopPropagation();
        }
        
        private void OnDragEnd(EventBase evt)
        {
            if (_selectedSwatch == null) return;

            // 非表示状態でドラッグ終了したら削除
            if (!IsAttachedSwatch(_selectedSwatch))
            {
                RemoveSwatch(_selectedSwatch);
            }

            evt.StopPropagation();
        }

        private void OnKeyDownOnContainer(KeyDownEvent evt)
        {
            // Deleteキーで選択中のスウォッチを削除
            if(evt.keyCode == KeyCode.Delete)
            {
                if (_showedSwatches.Count > 1 
                    && _selectedSwatch != null
                    // ドラッグ中で一時的に消えている場合はDeleteキーは無視
                    && IsAttachedSwatch(_selectedSwatch))
                {
                    RemoveSwatch(_selectedSwatch);
                }

                evt.StopPropagation();
            }
        }


        private void AttachSwatch(GradientKeysSwatch swatch)
        {
            _showedSwatches.Add(swatch);
            _container.Add(swatch.visualElement);
        }
        
        private void DetachSwatch(GradientKeysSwatch swatch)
        {
            _showedSwatches.Remove(swatch);
            swatch.visualElement.RemoveFromHierarchy();
        }

        private bool IsAttachedSwatch(GradientKeysSwatch swatch) => _showedSwatches.Contains(swatch);

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
            UnselectSwatchWithoutNotify();

            _selectedSwatch = swatch;
            _selectedSwatch.visualElement.Focus();
            _selectedSwatch.visualElement.BringToFront();
            
            _onSelectedSwatchChanged?.Invoke(_selectedSwatch);
        }
        
        private void UnselectSwatchWithoutNotify()
        {
            if (_selectedSwatch == null) return;

            _selectedSwatch.visualElement.Blur();
            _selectedSwatch = null;
        }
        
        private void AddSwatch(GradientKeysSwatch swatch)
        {
            if (swatch == null) return;

            AttachSwatch(swatch);
            _onAddSwatch?.Invoke(swatch);
        }
        
        private void RemoveSwatch(GradientKeysSwatch swatch)
        {
            if (swatch == null) return;

            if (swatch == _selectedSwatch)
            {
                UnselectSwatchWithoutNotify();
            }
            
            DetachSwatch(swatch);
            _onRemoveSwatch?.Invoke(swatch);
        }

        private void OnSwatchValueChanged()
        {
            // 非表示中の値の変化は通知しない
            if ( IsAttachedSwatch(_selectedSwatch))
            {
                _onSwatchValueChanged?.Invoke(_selectedSwatch);
            }
        }
    }
}
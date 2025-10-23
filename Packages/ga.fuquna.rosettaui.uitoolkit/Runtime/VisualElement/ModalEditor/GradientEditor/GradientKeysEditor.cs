using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.UndoSystem;
using RosettaUI.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    /// <summary>
    /// GradientEditorのキーを編集するクラス
    /// Color/Alphaのキーを編集する
    /// </summary>
    public class GradientKeysEditor
    {
        #region Snapshot Class
        
        public class Snapshot : ObjectPoolItem<Snapshot>
        {
            public int SelectedSwatchId { get; private set; }
            public readonly List<GradientKeysSwatch.Snapshot> swatchSnapshots = new();


            public void Initialize(int selectedId, IEnumerable<GradientKeysSwatch> swatches)
            {
                Initialize(selectedId, swatches.Select(s => s.GetSnapshot()));
            }

            private void Initialize(int selectedId, IEnumerable<GradientKeysSwatch.Snapshot> snapshots)
            {
                SelectedSwatchId = selectedId;
                swatchSnapshots.Clear();
                swatchSnapshots.AddRange(snapshots);
            }

            public void CopyTo(Snapshot snapshot)
            {
                snapshot.Initialize(SelectedSwatchId, swatchSnapshots);
            }

            public override void Dispose()
            {
                base.Dispose();
                SelectedSwatchId = -1;
                swatchSnapshots.Clear();
            }
            
            public override string ToString()
            {
                return $"SelectedId: {SelectedSwatchId}, Swatches: [{string.Join(", ", swatchSnapshots)}]";
            }
        }
        
        #endregion
        
        
        public static int MaxKeyNum { get; set; } = 8;
        
        private readonly GradientEditor _gradientEditor;
        private readonly VisualElement _container;
        private readonly List<GradientKeysSwatch> _showedSwatches = new();
        private readonly bool _isAlpha;
        private readonly Snapshot _dragStartSnapshot = new();
        
        private GradientKeysSwatch _selectedSwatch;
        
        public IReadOnlyList<GradientKeysSwatch> ShowedSwatches => _showedSwatches;
        
        public GradientKeysEditor(GradientEditor gradientEditor, VisualElement container, bool isAlpha
            )
        {
            _gradientEditor = gradientEditor;
            _container = container;
            _isAlpha = isAlpha;
            
            container.RegisterCallback<KeyDownEvent>(OnKeyDownOnContainer);
            container.AddManipulator(new DragManipulator(OnDragStart, OnDrag, OnDragEnd));
        }

        public void Initialize(IEnumerable<GradientKeysSwatch> showedSwatches)
        {
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
            _showedSwatches.Sort((a, b) => a.TimePercent.CompareTo(b.TimePercent));
            
            TakeSnapshot(_dragStartSnapshot);
            
            var localPos = _container.WorldToLocal(evt.position);
            var swatch = _showedSwatches.FirstOrDefault(s => s.visualElement.localBound.Contains(localPos));
            
            // 無かったら新規追加
            if (swatch == null)
            {
                swatch = CreateSwatch(localPos.x);
                if (swatch != null)
                {
                    AddSwatch(swatch);
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

            
            RecordUndoKeysSnapshot();
            
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
            var color = _gradientEditor.Evaluate(t);
            
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

        public void SelectSwatch(GradientKeysSwatch swatch)
        {
            UnselectSwatchWithoutNotify();

            _selectedSwatch = swatch;
            _gradientEditor.UpdateSelectedSwatchField(_selectedSwatch);
        }

        private void UnselectSwatch()
        {
            UnselectSwatchWithoutNotify();
            _gradientEditor.UpdateSelectedSwatchField(null);
        }
        
        private void UnselectSwatchWithoutNotify()
        {
            _selectedSwatch = null;
        }

        
        private void AddSwatch(GradientKeysSwatch swatch)
        {
            if (swatch == null) return;

            AttachSwatch(swatch);
            NotifyGradientKeysChanged();
            
            // 追加直後にフォーカスを当てる
            swatch.visualElement.schedule.Execute(() =>
            {
                SelectSwatch(swatch);
            });
        }
        
        private void RemoveSwatch(GradientKeysSwatch swatch)
        {
            if (swatch == null) return;

            if (swatch == _selectedSwatch)
            {
                UnselectSwatch();
            }
            
            DetachSwatch(swatch);
            NotifyGradientKeysChanged();
        }

        private void OnSwatchValueChanged()
        {
            // 非表示中の値の変化は通知しない
            if ( IsAttachedSwatch(_selectedSwatch))
            {
                _gradientEditor.UpdateSelectedSwatchField(_selectedSwatch);
                NotifyGradientKeysChanged();
            }
        }

        private void NotifyGradientKeysChanged()
        {
            _gradientEditor.OnGradientKeysChanged();
        }
        
        
        #region Undo

        public void TakeSnapshot(Snapshot snapshot)
        {
            snapshot.Initialize(_gradientEditor.GetSelectedSwatchId(), _showedSwatches);
        }
        
        public void RestoreSnapshotWithoutNotify(Snapshot snapshot)
        {
            Initialize(snapshot.swatchSnapshots.Select(GradientKeysSwatch.CreateFromSnapshot));
            _gradientEditor.SelectSwatchById(snapshot.SelectedSwatchId);
        }
        
        public void RestoreSnapshot(Snapshot snapshot)
        {
            RestoreSnapshotWithoutNotify(snapshot);
            NotifyGradientKeysChanged();
        }
        
        /// <summary>
        /// ドラッグ終了時にドラッグ前に戻せるようにスナップショットを記録する
        /// </summary>
        private void RecordUndoKeysSnapshot()
        {
            var before = Snapshot.GetPooled();
            var after = Snapshot.GetPooled();
            
            _dragStartSnapshot.CopyTo(before);
            TakeSnapshot(after);

            Undo.RecordValueChange(
                $"{nameof(GradientKeysEditor)} {(_isAlpha ? "Alpha" : "Color")} Keys Change",
                before,
                after,
                RestoreSnapshot
            );
        }
        
        #endregion
    }
}
using System;
using System.Collections.Generic;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class PrefixLabelWidthCalculator
    {
        #region Type Define
        
        // サイズを決める最も親のVisualElement（Runtime時はWindow、Editorではコンテナ）を監視して
        // 初期状態のWindowサイズと現在のサイズの変化率を通知する
        // プレフィックスラベルのサイズはこの変化率に追従することで以下の効果を狙っている
        // ・個々が複雑な構成の複数のエレメントで縦に揃う配置ができる
        // ・ラベルもWindowを広げることで見えるようにする
        private class RootData
        {
            public event Action<RootData> onWidthRateChanged;
            public float WidthRate { get; private set; } = 1f;
            
            private float? _initialWidth;
            private int _windowResizeFrame;
            
            public RootData(VisualElement root)
            {
                // 初回GeometryChangedEventでinitialWidthを登録
                // １フレーム内で内容によってサイズがかわるので複数回呼ばれる
                var initialFrame = Time.frameCount;
                
                root.RegisterCallbackUntil<GeometryChangedEvent>(
                    // 次フレーム以降なら解除
                    () => initialFrame < Time.frameCount,
                    (evt) =>
                    {
                        _initialWidth = evt.newRect.width;
                    }
                );

                root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                
                // WindowResize中（FoldのOn･Offなど）ではWidthRateは更新しない
                // WidthRateはユーザーが手動でResizeしたときのみ変更したい
                // （勝手にラベル幅が変わってほしくない）
                root.RegisterCallback<RequestResizeWindowEvent>(evt =>
                {
                    _windowResizeFrame = Time.frameCount;
                });
            }

            private void OnGeometryChanged(GeometryChangedEvent evt)
            {
                if (!_initialWidth.HasValue ) return;
                if (_windowResizeFrame == Time.frameCount) return;
                    
                // 幅以外の変化は無視
                if ( evt.newRect.width <=0 || Mathf.Approximately(evt.oldRect.width ,evt.newRect.width)) return;
                    
                var newWidthRate = evt.newRect.width / _initialWidth.Value;

                if (Mathf.Approximately(WidthRate, newWidthRate)) return;
                
                WidthRate = newWidthRate;
                onWidthRateChanged?.Invoke(this);
            }
        }
        
        #endregion


        private static readonly Dictionary<VisualElement, RootData> _rootDataTable = new();

        public static void Register(LabelElement label, VisualElement ve)
        {
            // TODO: OnAttachTOPanelなどあとで呼ば絵れるときに値が変化した場合の対応
            var labelWidthGlobal = LayoutSettings.LabelWidth;

            // パネル未登録なら登録時に再度呼ばれるようにして一旦終了
            if (ve.panel == null)
            {
                ve.RegisterCallbackOnce<AttachToPanelEvent>(_ =>
                {
                    Assert.IsNotNull(ve.parent);
                    Register(label, ve);
                });
                return;
            }

            var root = GetWidthRoot(ve);
            Assert.IsNotNull(root);

            if (!_rootDataTable.TryGetValue(root, out var rootData))
            {
                _rootDataTable[root] = rootData = new RootData(root);
            }
            
            // 非表示から表示に変化した場合
            ve.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                if (IsVisible(evt.oldRect) || !IsVisible(evt.newRect)) return;

                UpdateWidthWithRate(rootData);
            });

            rootData.onWidthRateChanged += UpdateWidthWithRate;
            
            
            #region Local Funcitons
            
            static bool IsVisible(in Rect rect)
            {
                return Mathf.Abs(rect.width) > 0f && Mathf.Abs(rect.height) > 0f;
            }
            
            void UpdateWidthWithRate(RootData rd)
            {
                if (!IsVisible(ve.layout)) return;
                if (rd?.WidthRate is not { } widthRate) return;

                ve.style.width = CalcLabelWidth(labelWidthGlobal * widthRate);
            }
            
            float CalcLabelWidth(float widthGlobal)
            {
                if (label.Parent == null)
                {
                    Debug.LogWarning($"Label parent is null. [{label.Value}]");
                }

                var marginLeft = ve.worldBound.xMin;

                for (var element = label.Parent;
                     element != null;
                     element = element.Parent)
                {
                    if (LayoutHint.IsIndentOrigin(element))
                    {
                        marginLeft -= UIToolkitBuilder.Instance.GetUIObj(element).worldBound.xMin;
                        break;
                    }
                }
                
                return widthGlobal - marginLeft;
            }
            
            #endregion
        }
        
        private static VisualElement GetWidthRoot(VisualElement ve)
        {
            while (true)
            {
                if (ve is Window || ve.parent == null) return ve;
                ve = ve.parent;
            }
        }
    }
}
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
            
            private float? _baseWidth;
            private bool _duringRequestResizeWindow;
            
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
                        _baseWidth = evt.newRect.width;
                    }
                );

                root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                
                // RequestResizeWindow中（FoldのOn/Offなど）ではWidthRateは更新しない
                // WidthRateはユーザーが手動でResizeしたときのみ変更したい
                // 勝手にラベル幅が変わってほしくない
                root.RegisterCallback<RequestResizeWindowEvent>(_ =>
                {
                    _duringRequestResizeWindow = true;
                    root.schedule.Execute(() => _duringRequestResizeWindow = false);
                });
            }

            private void OnGeometryChanged(GeometryChangedEvent evt)
            {
                if (!_baseWidth.HasValue ) return;

                // 幅以外の変化は無視
                if ( evt.newRect.width <=0 || Mathf.Approximately(evt.oldRect.width ,evt.newRect.width)) return;

                // RequestResizeWindowでサイズが変わった場合、以降のResizeでWidthが継続するように_baseWidthを更新しておく
                // currentWidth / baseWidthNew[?] = WidthRate
                // -> baseWidthNew = currentWidth / WidthRate
                if (_duringRequestResizeWindow)
                {
                    _baseWidth = evt.newRect.width / WidthRate;
                    return;
                }
                    
                var newWidthRate = evt.newRect.width / _baseWidth.Value;

                if (Mathf.Approximately(WidthRate, newWidthRate)) return;
                
                WidthRate = newWidthRate;
                onWidthRateChanged?.Invoke(this);
            }
        }
        
        #endregion


        private static readonly Dictionary<VisualElement, RootData> _rootDataTable = new();

        public static void Register(LabelElement label, VisualElement ve)
        {
            // パネル未登録なら登録時に再度呼ばれるようにして一旦終了
            if (ve.panel == null)
            {
                ve.RegisterCallbackOnce<AttachToPanelEvent>(OnAttachToPanelEventFirst);
                return;
            }

            var root = GetWidthRoot(ve);
            Assert.IsNotNull(root);

            if (!_rootDataTable.TryGetValue(root, out var rootData))
            {
                _rootDataTable[root] = rootData = new RootData(root);
            }

            var firstTime = true;

            // ラベルのサイズを変えるタイミングは３つ
            // 1. 初期化時可能であればとりあえず更新
            // 通常はサイズゼロの非表示状態のため効果がないが既存のVisualElementに再Bindされたときなどに対応
            UpdateWidthWithRate(rootData);
            
            // 2. 非表示から表示に変化した場合
            //  1.で対応されなかったがあとで可視になったとき 
            // VisualElementが新規作成された場合はこちらが呼ばれる
            ve.RegisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
            
            // 3. あとはRootのサイズが変化したとき
            rootData.onWidthRateChanged += UpdateWidthWithRate;


            // 削除
            label.GetViewBridge().onUnsubscribe += Unsubscribe;
            
            #region Local Funcitons

            void OnAttachToPanelEventFirst(AttachToPanelEvent _)
            {
                Assert.IsNotNull(ve.parent);
                Register(label, ve);
            }

            void OnGeometryChangedEvent(GeometryChangedEvent evt)
            {
                // 自身のWidthが変化してまたGeometryChangedEventが呼ばれるのを防ぐため
                // 可視になったときのみアップデート
                if (IsVisible(evt.oldRect) || !IsVisible(evt.newRect)) return;
                UpdateWidthWithRate(rootData);
            }
            
            void UpdateWidthWithRate(RootData rd)
            {
                if (!IsVisible(ve.layout)) return;
                if (rd?.WidthRate is not { } widthRate) return;
                
                var width = CalcLabelWidth(LayoutSettings.LabelWidth * widthRate);

                // 最低サイズ星
                if (width < 50f)
                {
                    // 初回で最小サイズ以下ならPrefixLabel扱いにはしない
                    // やたら右寄りのラベル
                    if (firstTime) Unsubscribe();
                    return;
                }

                firstTime = false;
                ve.style.width = width;
            }

            void Unsubscribe()
            {
                label.GetViewBridge().onUnsubscribe -= Unsubscribe;

                ve.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanelEventFirst);
                ve.UnregisterCallback<GeometryChangedEvent>(OnGeometryChangedEvent);
                rootData.onWidthRateChanged -= UpdateWidthWithRate;
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

                return widthGlobal - marginLeft / ve.worldTransform.m00;
            }
            
            #endregion
        }
        
        private static bool IsVisible(in Rect rect)
        {
            return Mathf.Abs(rect.width) > 0f && Mathf.Abs(rect.height) > 0f;
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
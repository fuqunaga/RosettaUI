using System;
using System.Collections.Generic;
using RosettaUI.Reactive;

namespace RosettaUI
{
    /// <summary>
    /// 動的に内容が変化するElement
    /// UI実装側はbuildの結果をいれるプレースホルダー的な役割
    ///
    /// UIのビルドはできるだけ遅延する
    /// - RegisterBuildUI()時にEnableなとき
    /// - Enable==trueになって_needBuildChildren==true（まだ子供のElementを生成していない）とき
    /// </summary>
    public class DynamicElement : ElementGroup
    {
        #region Static
        
        public static DynamicElement Create<T>(Func<T> readStatus, Func<T, Element> buildWithStatus,
            string displayName = null)
        {
            var status = readStatus();

            return new DynamicElement(
                () => buildWithStatus(status),
                e =>
                {
                    var newStatus = readStatus();
                    var rebuild = !EqualityComparer<T>.Default.Equals(newStatus, status);
                    if (rebuild) status = newStatus;
                    return rebuild;
                },
                displayName
            );
        }
        
        #endregion

        public event Action<DynamicElement> onBuildChildren;
        private event Action<DynamicElement> buildUI;
        
        
        private readonly Func<Element> _build;
        private readonly string _displayName;
        private readonly Func<DynamicElement, bool> _rebuildIf;
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        private bool _needBuildChildren = true;
        
        public override IEnumerable<Element> Contents
        {
            get
            {
                if (_needBuildChildren)
                {
                    _needBuildChildren = false;
                    BuildElement();
                }

                return base.Contents;
            }
        }

        public DynamicElement(Func<Element> build, Func<DynamicElement, bool> rebuildIf, string displayName = null)
        {
            _build = build;
            _rebuildIf = rebuildIf;
            _displayName = displayName;
            
            _binderTypeHistorySnapshot = BinderHistory.Snapshot.Create();

            enableRx.Subscribe(enable =>
            {
                if (enable && _needBuildChildren)
                {
                    BuildUI();
                }
            });
        }

        public override string DisplayName => string.IsNullOrEmpty(_displayName) ? base.DisplayName : _displayName;
        
        protected override void UpdateInternal()
        {
            CheckAndRebuild();
            base.UpdateInternal();
        }

        private void BuildElement()
        {
            using var applyScope = _binderTypeHistorySnapshot.GetApplyScope();
            SetElements(new[] {_build?.Invoke()});
            
            onBuildChildren?.Invoke(this);
        }

        public void CheckAndRebuild()
        {
            if (_rebuildIf?.Invoke(this) ?? false)
            {
                DestroyChildren(true);
                _needBuildChildren = true;
                BuildUI();
            }
        }

        private void BuildUI() => buildUI?.Invoke(this);

        public void RegisterBuildUI(Action<DynamicElement> action)
        {
            buildUI += action;
            
            if ( Enable) action?.Invoke(this);
        }
    }
}
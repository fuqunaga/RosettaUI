using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Reactive;

namespace RosettaUI
{
    /// <summary>
    /// 動的に内容が変化するElement
    /// UI実装側はbuildの結果をいれるプレースホルダー的な役割
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
        private event Action<DynamicElement> bindChildrenToView;
        
        private readonly Func<Element> _build;
        private readonly string _displayName;
        private readonly Func<DynamicElement, bool> _rebuildIf;
        private readonly BinderHistory.Snapshot _binderTypeHistorySnapshot;
        
        public DynamicElement(Func<Element> build, Func<DynamicElement, bool> rebuildIf, string displayName = null)
        {
            _build = build;
            _rebuildIf = rebuildIf;
            _displayName = displayName;
            
            _binderTypeHistorySnapshot = BinderHistory.Snapshot.Create();

            BuildElement();
            enableRx.Subscribe(enable =>
            {
                if (enable)
                {
                    CheckAndRebuild();
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
            while (Children.Any())
            {
                var child = Children.Last();
                RemoveChild(child, false);
            }

            using var applyScope = _binderTypeHistorySnapshot.GetApplyScope();
            SetElements(new[] {_build?.Invoke()});

            bindChildrenToView?.Invoke(this);
            onBuildChildren?.Invoke(this);
        }

        public void CheckAndRebuild()
        {
            if (!(_rebuildIf?.Invoke(this) ?? false)) return;
            BuildElement();
        }

        private void ClearBindView() => bindChildrenToView = null;
        
        protected override ElementViewBridge CreateViewBridge() => new DynamicElementViewBridge(this);
        
        public class DynamicElementViewBridge : ElementViewBridge
        {
            private DynamicElement Element => (DynamicElement)element;
            
            public DynamicElementViewBridge(DynamicElement element) : base(element)
            {
            }
            
            public void RegisterBindViewAndCallOnce(Action<DynamicElement> action)
            {
                Element.bindChildrenToView += action;
                action?.Invoke(Element);
            }

            public override void UnsubscribeAll()
            {
                base.UnsubscribeAll();
                Element.ClearBindView();
            }
        }
    }
    
           
    public static partial class ElementViewBridgeExtensions
    {
        public static DynamicElement.DynamicElementViewBridge GetViewBridge(this DynamicElement element) => (DynamicElement.DynamicElementViewBridge)element.ViewBridge;
    }
}
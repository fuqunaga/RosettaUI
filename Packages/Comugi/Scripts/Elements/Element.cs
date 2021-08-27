using Comugi.Reactive;
using System;

namespace Comugi
{
    /// <summary>
    /// Comugiの基本単位
    /// - UIの実装に依存しない
    /// - ElementをもとにBuilderが実装依存のUI実体を作る
    /// - アプリケーションはElement経由でUIにアクセスし、実体には触れない
    /// </summary>
    public abstract class Element
    {
        public readonly ReactiveProperty<bool> enableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<bool> interactableRx = new ReactiveProperty<bool>(true);
        public readonly ReactiveProperty<Layout> layoutRx = new ReactiveProperty<Layout>();

        public event Action<Element> onRebuild;
        public event Action<Element> onDestroy;

        public bool enable
        {
            get => enableRx.Value;
            set => enableRx.Value = value;
        }

        public bool interactable
        {
            get => interactableRx.Value;
            set => interactableRx.Value = value;
        }

        public Layout layout
        {
            get => layoutRx.Value;
            set => layoutRx.Value = value;
        }


        public ElementGroup parentGroup { get; internal set; }


        public virtual void Update()
        {
            if (enable) UpdateInternal();
        }

        protected virtual void UpdateInternal() { }

        public void Rebuild() => onRebuild?.Invoke(this);
        public void Destroy() => onDestroy?.Invoke(this);
    }
}
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
        public event Action<Element> onDestroy;

        protected bool _enable = true;

        public bool enable
        {
            get => _enable;
            set
            {
                _enable = value;
                ViewBridge.SetActive(this, _enable);
            }
        }

        protected bool _interactableSelf = true;

        public bool IsInteractable => interactableSelf && (parentGroup?.IsInteractable ?? true);
        public virtual bool interactableSelf
        {
            get => _interactableSelf;
            set
            {
                if (_interactableSelf != value)
                {
                    _interactableSelf = value;
                    NotifyInteractive();
                }
            }
        }

        internal virtual void NotifyInteractive()
        {
            ViewBridge.SetInteractive(this, IsInteractable);
        }


        Layout _layout;

        public Layout layout
        {
            get => _layout;
            set
            {
                if (!_layout.Equals(value))
                {
                    _layout = value;
                    ViewBridge.SetLayout(this, _layout);
                }
            }
        }

        public ElementGroup parentGroup { get; internal set; }


        public virtual void Update()
        {
            if (enable) UpdateInternal();
        }

        protected virtual void UpdateInternal() { }

        public void Rebuild()
        {
            ViewBridge.Rebuild(this);
        }

        public void Destroy()
        {
            onDestroy?.Invoke(this);
            ViewBridge.Destroy(this);
        }
    }
}
using System;


namespace Comugi
{
    public static class ViewBridge
    {
        static event Action<Element, bool> setActiveImpl;
        static event Action<Element, bool> setInteractiveImpl;
        static event Action<FoldElement, bool> setFoldOpenImpl;
        static event Action<Element, Layout> setLayoutImpl;
        static event Action<Element> rebuildImpl;
        static event Action<Element> destroyImpl;

        public static void Init(
            Action<Element, bool> setActive,
            Action<Element, bool> setInteractive,
            Action<FoldElement, bool> setFoldOpen,
            Action<Element, Layout> setLayout,
            Action<Element> rebuild, 
            Action<Element> destory)
        {
            setActiveImpl += setActive;
            setInteractiveImpl += setInteractive;
            setFoldOpenImpl += setFoldOpen;
            setLayoutImpl += setLayout;
            rebuildImpl += rebuild;
            destroyImpl += destory;
        }

  

        internal static void SetActive(Element element, bool active) => setActiveImpl?.Invoke(element, active);
        internal static void SetInteractive(Element element, bool interactive) => setInteractiveImpl?.Invoke(element, interactive);
        internal static void SetFoldOpen(FoldElement foldElement, bool isOpen) => setFoldOpenImpl?.Invoke(foldElement, isOpen);

        internal static void SetLayout(Element element, Layout layout) => setLayoutImpl?.Invoke(element, layout);

        internal static void Rebuild(Element element) => rebuildImpl(element);
        internal static void Destroy(Element element) => destroyImpl?.Invoke(element);

       
        public static void InitElementAfterBuild(Element element)
        {
            element.NotifyInteractive();

            if (element is FoldElement fold)
            {
                SetFoldOpen(fold, fold.isOpen);
            }

            SetLayout(element, element.layout);
        }
    }
}
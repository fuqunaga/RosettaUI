using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Comugi
{
    /// <summary>
    /// Top level interface
    /// </summary>
    public static class UI
    {
        #region Label

        public static Label Label(Label label) => label; // use implicit operator of Label
        public static Label Label(Func<string> readLabel) => readLabel;

        #endregion


        #region Field

        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null) => Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, onValueChanged);

        public static Element Field<T>(Label label, Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder == null) return null;
            binder.onValueChanged += onValueChanged;

            return Field(label, binder);
        }

        public static Element Field(Label label, IBinder binder)
        {
            var element = BinderToElement.CreateElement(binder);
            if (element == null) return null;

            if (label != null)
            {
                element = binder.IsOneliner()
                    ? Row(label, element)
                    : Fold(label, element) as Element;
            }

            SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion


        #region Slider

        public static Element Slider(Expression<Func<int>> targetExpression, int min = 0, int max = 100, Action<int> onValueChanged = null) => Slider<int>(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, min, max, onValueChanged);
        public static Element Slider(Expression<Func<float>> targetExpression, float min = 0f, float max = 1f, Action<float> onValueChanged = null) => Slider<float>(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, min, max, onValueChanged);

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null) => Slider<T>(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, min, max, onValueChanged);


        public static Element Slider(Label label, Expression<Func<int>> targetExpression, int min = 0, int max = 100, Action<int> onValueChanged = null) => Slider<int>(label, targetExpression, min, max, onValueChanged);
        public static Element Slider(Label label, Expression<Func<float>> targetExpression, float min = 0f, float max = 1f, Action<float> onValueChanged = null) => Slider<float>(label, targetExpression, min, max, onValueChanged);

        public static Element Slider<T>(Label label, Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder == null) return null;
            binder.onValueChanged += onValueChanged;
            return Slider(label, binder, ConstMinMaxGetter.Create((min, max)));
        }

        public static Element Slider(Label label, IBinder binder, IMinMaxGetter minMaxGetter)
        {
            var contents = binder.CreateSliderElement(minMaxGetter);
            if (contents == null) return null;

            var element = label == null
                ? contents
                : Row(label, contents);

            SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion


        #region Button

        public static ButtonElement Button(string name, Action onClick) => new ButtonElement(ConstGetter.Create(name), onClick);
        public static ButtonElement Button(Func<string> readName, Action onClick) => new ButtonElement(Getter.Create(readName), onClick);

        #endregion


        #region List


        public static Element List<TItem, TValue>(Label label, List<TItem> list, Func<TItem, TValue> readItemValue, Action<TItem, TValue> onItemValueChanged, Func<TItem, string> createItemLabel = null)
            where TItem : class
        {
            return List(label,
                list,
                (binder, defaultLabelString) =>
                {
                    var childBinder = new ChildBinder<TItem, TValue>(binder,
                        readItemValue,
                        (item, value) =>
                        {
                            onItemValueChanged?.Invoke(item, value);
                            return item;
                        }
                        );

                    var itemLabel = ((createItemLabel != null) && !binder.IsNull)
                        ? Label(() => createItemLabel(binder.Get()))
                        : (Label)defaultLabelString;

                    return Field(itemLabel, childBinder);
                }
                );

        }

        public static Element List<T>(Label label, List<T> list, Func<BinderBase<T>, string, Element> createItemElement = null)
        {
            Func<IBinder, string, Element> createItemElementIBinder = null;
            if (createItemElement != null)
            {
                createItemElementIBinder = (ibinder, itemLabel) => createItemElement(ibinder as BinderBase<T>, itemLabel);
            }

            var element = BinderToElement.CreateListElement(ConstGetter.Create(list), createItemElementIBinder);
            return Fold(label, element);

        }

        #endregion

        #region Dropdown



        public static Element Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options, Action<int> onValueChanged = null) => Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options, onValueChanged);

        public static Element Dropdown(Label label, Expression<Func<int>> targetExpression, IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            var optionsGetter = ConstGetter.Create(options);
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder == null) return null;
            binder.onValueChanged += onValueChanged;

            Element element = new Dropdown(binder, optionsGetter);

            if (label != null) element = Row(label, element);

            SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion

        #region Row/Column

        public static Row Row(params Element[] elements) => new Row(elements);
        public static Row Row(IEnumerable<Element> elements) => new Row(elements);
        public static Column Column(params Element[] elements) => new Column(elements);
        public static Column Column(IEnumerable<Element> elements) => new Column(elements);

        #endregion

        #region Fold

        public static FoldElement Fold(Label label, params Element[] elements) => Fold(label, elements as IEnumerable<Element>);
        public static FoldElement Fold(Label label, IEnumerable<Element> elements) => new FoldElement(label, Column(elements));

        #endregion

        #region Window

        public static Window Window(params Element[] elements) => new Window(elements);
        public static Window Window(IEnumerable<Element> elements) => new Window(elements);

        #endregion

        #region FindObject

        public static DynamicElement FindObjectObserverElement<T>(bool rebuildIfDisabled = true)
            where T : Behaviour, IElementCreator
        {
            return FindObjectObserverElement<T>((t) => t.CreateElement(), typeof(T).Name, rebuildIfDisabled);
        }

        public static DynamicElement FindObjectObserverElement<T>(Func<T, Element> build, string displayName = null, bool rebuildIfDisabled = true)
            where T : Behaviour
        {
            T target = null;
            float lastCheckTime = Time.realtimeSinceStartup;
            var interval = UnityEngine.Random.Range(1f, 1.5f); // 起動時に多くのFindObjectObserverElementが呼ばれるとFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす

            Func<bool> checkTargetEnable = () => (target != null) && !(rebuildIfDisabled && !target.isActiveAndEnabled);

            return new DynamicElement(
               build: () => (target != null) ? build?.Invoke(target) : null,
               rebuildIf: (e) =>
               {
                   if (!checkTargetEnable())
                   {
                       var t = Time.realtimeSinceStartup;
                       if (t - lastCheckTime > interval)
                       {
                           lastCheckTime = t;
                           target = Object.FindObjectOfType<T>();
                           return true;
                       }
                   }

                   return false;
               },
               displayName
               );
        }


        #endregion


        static void SetInteractableWithBinder(Element element, IBinder binder) => element.interactableSelf = !binder.IsReadOnly;


    }
}
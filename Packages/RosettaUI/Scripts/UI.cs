using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace RosettaUI
{
    /// <summary>
    /// Top level interface
    /// </summary>
    public static class UI
    {
        #region Button

        public static ButtonElement Button(LabelElement label, Action onClick)
        {
            return new ButtonElement(label?.getter, onClick);
        }

        #endregion


        #region Label

        public static LabelElement Label(LabelElement label) => label;
        public static LabelElement Label(Func<string> readLabel) => readLabel;

        #endregion


        #region Field

        public static Element Field<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Field(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, onValueChanged);
        }

        public static Element Field<T>(LabelElement label, Expression<Func<T>> targetExpression,
            Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return Field(label, binder);
        }

        public static Element Field(LabelElement label, IBinder binder)
        {
            var element = BinderToElement.CreateFieldElement(label, binder);
            if (element != null) SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element FieldReadOnly<T>(Func<T> getValue) => FieldReadOnly(null, getValue);
        
        public static Element FieldReadOnly<T>(LabelElement label, Func<T> getValue) =>
            Field(label, Binder.Create(getValue, null));
        

        #endregion


        #region Slider

        public static Element Slider<T>(Expression<Func<T>> targetExpression, T max, Action<T> onValueChanged = null)
        {
            return Slider(targetExpression, default, max, onValueChanged);
        }


        public static Element Slider<T>(Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Slider(ExpressionUtility.CreateLabelString(targetExpression),
                targetExpression,
                null,
                null,
                onValueChanged);
        }


        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T max, Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, T min, T max, Action<T> onValueChanged = null)
        {
            return Slider(label,
                targetExpression,
                ConstGetter.Create(min),
                ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element Slider<T>(LabelElement label, Expression<Func<T>> targetExpression, Action<T> onValueChanged = null)
        {
            return Slider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element Slider<T>(LabelElement label,
            Expression<Func<T>> targetExpression,
            IGetter minGetter,
            IGetter maxGetter,
            Action<T> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            if (minGetter == null || maxGetter == null)
            {
                var (rangeMinGetter, rangeMaxGetter) = CreateMinMaxGetterFromRangeAttribute(targetExpression);
                minGetter ??= rangeMinGetter;
                maxGetter ??= rangeMaxGetter;
            }

            return Slider(label, binder, minGetter, maxGetter);
        }

        public static Element Slider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }

        #endregion


        #region MinMax Slider

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(targetExpression, default, max, onValueChanged);
        }


        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression, T min, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression,
                ConstGetter.Create(min), ConstGetter.Create(max), onValueChanged);
        }

        public static Element MinMaxSlider<T>(Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, null, null,
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T max,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, default, max, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression, T min,
            T max, Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, ConstGetter.Create(min), ConstGetter.Create(max),
                onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label, Expression<Func<MinMax<T>>> targetExpression,
            Action<MinMax<T>> onValueChanged = null)
        {
            return MinMaxSlider(label, targetExpression, null, null, onValueChanged);
        }

        public static Element MinMaxSlider<T>(LabelElement label,
            Expression<Func<MinMax<T>>> targetExpression,
            IGetter<T> minGetter,
            IGetter<T> maxGetter,
            Action<MinMax<T>> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);
            return MinMaxSlider(label, binder, minGetter, maxGetter);
        }

        public static Element MinMaxSlider(LabelElement label, IBinder binder, IGetter minGetter, IGetter maxGetter)
        {
            var contents = BinderToElement.CreateMinMaxSliderElement(label, binder, minGetter, maxGetter);
            if (contents == null) return null;

            SetInteractableWithBinder(contents, binder);

            return contents;
        }

        #endregion


        #region List

        public static Element List<T>(Expression<Func<IList<T>>> targetExpression, Func<IBinder<T>, int, Element> createItemElement = null)
        {
            var labelString = ExpressionUtility.CreateLabelString(targetExpression);
            return List(labelString, targetExpression, createItemElement);
        }
        
        public static Element List<T>(LabelElement label, Expression<Func<IList<T>>> targetExpression, Func<IBinder<T>, int, Element> createItemElement = null)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            var createItemElementIBinder = createItemElement == null
                ? (Func<IBinder, int, Element>)null
                : (ib, idx) => createItemElement(ib as IBinder<T>, idx);

            return List(label, binder, createItemElementIBinder);
        }

        public static Element List(LabelElement label, IBinder listBinder, Func<IBinder, int, Element> createItemElement = null)
        {
            var isReadOnly = ListBinder.IsReadOnly(listBinder);

            var countFieldWidth = 50f;
            var field = Field("",
                () => ListBinder.GetCount(listBinder),
                isReadOnly ? (Action<int>) null : (count) => ListBinder.SetCount(listBinder, count)
            ).SetWidth(countFieldWidth);
     
            var buttonWidth = 30f;
            var buttons = isReadOnly
                ? null
                : Row(
                    Button("＋", () => ListBinder.AddItemAtLast(listBinder)).SetWidth(buttonWidth),
                    Button("－", () => ListBinder.RemoveItemAtLast(listBinder)).SetWidth(buttonWidth)
                ).SetJustify(Style.Justify.End);

            return Fold(
                barLeft: label,
                barRight: Row(field),
                elements: new[]
                {
                    Box(
                        List(listBinder, createItemElement),
                        buttons
                    )
                }
            );
        }
        
        public static Element List(IBinder listBinder, Func<IBinder, int, Element> createItemElement = null)
        {
            return DynamicElementOnStatusChanged(
                readStatus: () => ListBinder.GetCount(listBinder),
                build: _ =>
                {
                    createItemElement ??= ((binder, idx) => Field("Item " + idx, binder));

                    var itemBinderToElement = createItemElement;

                    if (!ListBinder.IsReadOnly(listBinder))
                    {
                        itemBinderToElement = (binder,idx) => {
                            var element = Popup(
                                createItemElement(binder, idx),
                                () => new[]
                                {
                                    new MenuItem("Add Element", () => ListBinder.DuplicateItem(listBinder, idx)),
                                    new MenuItem("Remove Element", () => ListBinder.RemoveItem(listBinder, idx)),
                                }
                            );
                            
                            return element;
                        };
                    }

                    return Column(
                        ListBinder.CreateItemBinders(listBinder).Select(itemBinderToElement)
                    );
                });
        }
        
        #endregion


        #region PopupMenu

        public static PopupMenuElement Popup(Element childElement, Func<IEnumerable<MenuItem>> createMenuItems)
        {
            return new PopupMenuElement(childElement, createMenuItems);
        }
        
        
        #endregion
        
        
        #region Dropdown

        public static DropdownElement Dropdown(Expression<Func<int>> targetExpression, IEnumerable<string> options,
            Action<int> onValueChanged = null)
        {
            return Dropdown(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, options,
                onValueChanged);
        }

        public static DropdownElement Dropdown(LabelElement label, Expression<Func<int>> targetExpression,
            IEnumerable<string> options, Action<int> onValueChanged = null)
        {
            var binder = CreateBinder(targetExpression, onValueChanged);

            var element = new DropdownElement(label, binder, options);

            SetInteractableWithBinder(element, binder);

            return element;
        }

        #endregion
        
        #region Space

        public static SpaceElement Space() => new SpaceElement();
        
        #endregion


        #region Row/Column/Box/ScrollView/Indent

        public static Row Row(params Element[] elements) => Row(elements.AsEnumerable());

        public static Row Row(IEnumerable<Element> elements)
        {
            return new Row(elements);
        }

        public static Column Column(params Element[] elements) => Column(elements.AsEnumerable());

        public static Column Column(IEnumerable<Element> elements)
        {
            return new Column(elements);
        }

        public static BoxElement Box(params Element[] elements) => Box(elements.AsEnumerable());

        public static BoxElement Box(IEnumerable<Element> elements)
        {
            return new BoxElement(elements);
        }

        public static ScrollViewElement ScrollView(params Element[] elements) => ScrollView(elements.AsEnumerable());

        public static ScrollViewElement ScrollView(IEnumerable<Element> elements)
        {
            return new ScrollViewElement(elements);
        }

        public static IndentElement Indent(params Element[] elements) => Indent(elements.AsEnumerable());
        
        public static IndentElement Indent(IEnumerable<Element> elements)
        {
            return new IndentElement(elements);
        }
        #endregion


        #region Fold

        public static FoldElement Fold(LabelElement label, params Element[] elements) => Fold(label,  null, elements);

        public static FoldElement Fold(LabelElement label, IEnumerable<Element> elements) => Fold((Element)label, elements);

        public static FoldElement Fold(Element barLeft, Element barRight, IEnumerable<Element> elements)
        {
            barRight?.SetJustify(Style.Justify.End);
            var bar = (barLeft != null || barRight != null)
                ? Row(barLeft, barRight)
                : null;

            return Fold(bar, elements);
        }

        public static FoldElement Fold(Element bar, IEnumerable<Element> elements)
        {
            return new FoldElement(bar, elements);
        }

        #endregion


        #region DynamicElement

        public static DynamicElement DynamicElementIf(Func<bool> trigger, Func<Element> build)
        {
            return DynamicElementOnStatusChanged(
                trigger,
                flag => flag ? build() : null
            );
        }

        public static DynamicElement DynamicElementOnStatusChanged<T>(Func<T> readStatus, Func<T, Element> build)
            where T : IEquatable<T>
        {
            return DynamicElement.Create(readStatus, build);
        }

        public static DynamicElement DynamicElementOnTrigger(Func<DynamicElement, bool> rebuildIf, Func<Element> build)
        {
            return new DynamicElement(build, rebuildIf);
        }

        #endregion

        
        #region Window

        public static WindowElement Window(params Element[] elements)
        {
            return Window(null, elements);
        }

        public static WindowElement Window(LabelElement title, params Element[] elements)
        {
            return new WindowElement(title, elements);
        }

        public static WindowElement Window(LabelElement title, IEnumerable<Element> elements)
        {
            return new WindowElement(title, elements);
        }

        #endregion


        #region Window Launcher

        public static WindowLauncherElement WindowLauncher(WindowElement window)
        {
            return WindowLauncher(null, window);
        }

        public static WindowLauncherElement WindowLauncher(LabelElement title, WindowElement window)
        {
            var label = title ?? window.bar.FirstLabel();
            return new WindowLauncherElement(label, window);
        }

        public static WindowLauncherElement WindowLauncher<T>(LabelElement title = null)
            where T : Object
        {
            return WindowLauncher(title, typeof(T));
        }

        public static WindowLauncherElement WindowLauncher(params Type[] types) => WindowLauncher(null, types);

        public static WindowLauncherElement WindowLauncher(LabelElement title, params Type[] types)
        {
            Assert.IsTrue(types.Any());

            var elements = types.Select(FieldIfObjectFound).ToList();
            title ??= types.First().ToString().Split('.').LastOrDefault();
            var window = Window(title, elements);
            
#if true
            var launcher = WindowLauncher(window);
            launcher.UpdateWhenDisabled = true;
            launcher.onUpdate += _ =>
            {
                if (!window.Enable)
                {
                    window.Update();
                }
                
                var hasContents = elements.Any(dynamicElement => dynamicElement.Contents.Any());
                launcher.Enable = hasContents;
            };
            launcher.onDestroy += _ => window.Destroy();

            return launcher;
#else
            return DynamicElementIf(
                trigger: () =>
                {
                    var hasContents = elements.Any(dynamicElement => dynamicElement.Contents.Any());
                    
                    Debug.Log($"{title.Value} hasContents[{hasContents}]");
                    
                    return hasContents;
                },
                build: () => WindowLauncher(window)
            );
#endif
        }

        #endregion
        
        
        #region FindObject

        public static DynamicElement FieldIfObjectFound<T>()
            where T : Behaviour, IElementCreator
        {
            return FieldIfObjectFound(typeof(T));
        }

        public static DynamicElement FieldIfObjectFound(Type type)
        {
            return DynamicElementFindObject(type, obj => Field(null, () => obj));
        }

        
        public static DynamicElement DynamicElementFindObject<T>(Func<T, Element> build)
            where T : Object
        {
            return DynamicElementFindObject(typeof(T), (o) => build?.Invoke((T) o));
        }

        public static DynamicElement DynamicElementFindObject(Type type, Func<Object, Element> build)
        {
            Assert.IsTrue(typeof(Object).IsAssignableFrom(type));

            Object target = null;
            var lastCheckTime = 0f;
            // 起動時に多くのFindObjectObserverElementが呼ばれるとFindObject()を呼ぶタイミングがかぶって重いのでランダムで散らす
            var interval = Random.Range(1f, 1.5f);

            return DynamicElementIf(
                trigger: () =>
                {
                    if (target == null)
                    {
                        var t = Time.realtimeSinceStartup;
                        if (t - lastCheckTime > interval)
                        {
                            lastCheckTime = t;
                            target = Object.FindObjectOfType(type);
                        }
                    }

                    return target != null && !(target is Behaviour {isActiveAndEnabled: false});
                },
                build: () => build?.Invoke(target)
            );
        }
        


        #endregion


        static IBinder<T> CreateBinder<T>(Expression<Func<T>> targetExpression, Action<T> onValueChanged)
        {
            var binder = ExpressionUtility.CreateBinder(targetExpression);
            if (binder != null)
            {
                binder.onValueChanged += onValueChanged;
            }

            return binder;
        }


        static (IGetter<T>, IGetter<T>) CreateMinMaxGetterFromRangeAttribute<T>(Expression<Func<T>> targetExpression)
        {
            var rangeAttribute = typeof(IConvertible).IsAssignableFrom(typeof(T))
                ? ExpressionUtility.GetAttribute<T, RangeAttribute>(targetExpression)
                : null;

            return RangeUtility.CreateGetterMinMax<T>(rangeAttribute);
        }


        private static void SetInteractableWithBinder(Element element, IBinder binder)
        {
            element.Interactable = !binder.IsReadOnly;
        }
    }
}
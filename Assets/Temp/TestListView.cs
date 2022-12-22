using System.Collections.Generic;
using System.Linq;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEngine;
using UnityEngine.UIElements;

public class TestListView : MonoBehaviour
{
    void Start()
    {
        var document = GetComponent<UIDocument>();

        var root = document.rootVisualElement;
        root.Add(CreateElement());
    }

    private VisualElement CreateElement()
    {
        List<(int id, int value)> itemSource = Enumerable.Range(0, 1000).Select(i => (idx: i, value: i)).ToList();
        var heightTable = Enumerable.Range(0, itemSource.Count).Select(_ => Random.Range(30f, 100f)).ToList();

        // var ret = new ListViewCustom()
        var ret = new ListView()
        {
            style =
            {
                width = 1200f,
                height = 800f,
                backgroundColor = Color.gray
            },
            virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
            reorderable = true,
            reorderMode = ListViewReorderMode.Animated,
            showAddRemoveFooter = true,
            makeItem = MakeItem,
            bindItem = BindItem,
            unbindItem = UnbindItem
        };


        ret.itemsSource = itemSource;

        return ret;


        void OnAttachPanel(GeometryChangedEvent evt)
        {
            ret.UnregisterCallback<GeometryChangedEvent>(OnAttachPanel);
            ret.itemsSource = itemSource;
        }

        VisualElement MakeItem() => new IntegerField();

        void BindItem(VisualElement ve, int idx)
        {
            if (ve is not IntegerField integerField) return;
            var id = itemSource[idx].id;
            integerField.label = id.ToString();
            integerField.value = itemSource[idx].value;

            integerField.style.height = heightTable[id];
        }
        
        void UnbindItem(VisualElement ve, int idx)
        {}
    }
}

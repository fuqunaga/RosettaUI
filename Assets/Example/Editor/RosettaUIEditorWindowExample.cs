using System;
using System.Collections.Generic;
using System.Linq;
using RosettaUI;
using RosettaUI.Editor;
using RosettaUI.Example;
using RosettaUI.UIToolkit.Editor;
using UnityEngine;

public class RosettaUIEditorWindowExample : RosettaUIEditorWindowUIToolkit
{
    [UnityEditor.MenuItem("RosettaUI/RosettaUIEditorWindowExample")]
    public static void ShowExample()
    {
        var wnd = GetWindow<RosettaUIEditorWindowExample>();
        wnd.titleContent = new GUIContent(nameof(RosettaUIEditorWindowExample));
    }

    public GameObject gameObject;
    private Dictionary<Type, (ButtonElement button, Element element)> _typeToElementSet;
    
    protected override Element CreateElement()
    {
        var types = RosettaUIExample.ExampleTypes;
        var currentType = types.First();

        _typeToElementSet = types.ToDictionary(
            type => type,
            type => (
                UI.Button(
                    $"<align=left>{type.ToString().Split('.').Last()}</align>",
                    () => SetCurrentType(type)
                ),
                (Element)UI.FieldIfObjectFound(type, includeInactive: true)
            )
        );

        UpdateElements();
        
        return UI.ScrollView(ScrollViewType.VerticalAndHorizontal, new []
        {
            UI.Row(
                UI.Column(
                    _typeToElementSet.Values.Select(pair => pair.button)
                ).SetWidth(200f),
                UI.Page(
                    UI.Box(
                        _typeToElementSet.Values.Select(pair => pair.element)
                    )
                )
            )
        });
        
        
        void SetCurrentType(Type type)
        {
            if (type == currentType) return;
            currentType = type;
            UpdateElements();
        }

        void UpdateElements()
        {
            foreach (var (type, (button, e)) in _typeToElementSet)
            {
                var isCurrent = type == currentType;
                button.SetBackgroundColor(isCurrent ? Color.gray : null);
                e.Enable = isCurrent;
            }
        }
    }

    private Tab CreateUIEditorTab()
    {
        return Tab.Create(
            nameof(UIEditor),
            () => UI.Column(
                ExampleTemplate.FunctionColumn(nameof(UIEditor), nameof(UIEditor.ObjectField),
                    UIEditor.ObjectField(() => gameObject)
                ),
                ExampleTemplate.FunctionColumn(nameof(UIEditor), nameof(UIEditor.ObjectFieldReadOnly),
                    UIEditor.ObjectFieldReadOnly(() => gameObject)
                )
            )
        );
    }
}

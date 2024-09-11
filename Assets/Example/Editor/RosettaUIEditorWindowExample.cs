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
    private Dictionary<string, (ButtonElement button, Element element)> _typeNameToElementSet;
    private string _currentTypeName;
    
    protected override Element CreateElement()
    {
        var typeAndNames = RosettaUIExample.ExampleTypes.Select(type => (type, name: type.ToString().Split('.').Last()));
        

        _typeNameToElementSet = typeAndNames.ToDictionary(
            typeAndName => typeAndName.name,
            typeAndName => (
                CreateTypeButton(typeAndName.name),
                (Element)UI.FieldIfObjectFound(typeAndName.type, includeInactive: true)
            )
        );

        _typeNameToElementSet[nameof(UIEditor)] = (
            CreateTypeButton(nameof(UIEditor)),
            CreateUIEditorField()
        );

        SetCurrentTypeName(_typeNameToElementSet.Keys.First());
        
        
        return UI.ScrollView(ScrollViewType.VerticalAndHorizontal, new []
        {
            UI.Row(
                UI.Column(
                    _typeNameToElementSet.Values.Select(pair => pair.button)
                ).SetWidth(200f),
                UI.Page(
                    UI.Box(
                        _typeNameToElementSet.Values.Select(pair => pair.element)
                    )
                )
            )
        });


        ButtonElement CreateTypeButton(string typeName)
        {
            return UI.Button(
                $"<align=left>{typeName}</align>",
                () => SetCurrentTypeName(typeName)
            );
        }
        
        void SetCurrentTypeName(string typeName)
        {
            if (_currentTypeName == typeName) return;
            _currentTypeName = typeName;
            UpdateElements();
        }

        void UpdateElements()
        {
            foreach (var (typeName, (button, e)) in _typeNameToElementSet)
            {
                var isCurrent = typeName == _currentTypeName;
                button.SetBackgroundColor(isCurrent ? Color.gray : null);
                e.Enable = isCurrent;
            }
        }
    }

    private Element CreateUIEditorField()
    {
        return UI.Column(
            ExampleTemplate.FunctionColumn(nameof(UIEditor), nameof(UIEditor.ObjectField),
                UIEditor.ObjectField(() => gameObject)
            ),
            ExampleTemplate.FunctionColumn(nameof(UIEditor), nameof(UIEditor.ObjectFieldReadOnly),
                UIEditor.ObjectFieldReadOnly(() => gameObject)
            )
        );
    }
}

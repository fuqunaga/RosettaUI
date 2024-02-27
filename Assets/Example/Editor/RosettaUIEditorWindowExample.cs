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
    
    protected override Element CreateElement()
    {
        var types = RosettaUIExample.ExampleTypes;
        
        return UI.Tabs(
            types.Select(type =>
            {
                var tabName = type.ToString().Split('.').Last();
                return Tab.Create(tabName, () => UI.FieldIfObjectFound(type, false, true));
            }).Concat(new[]{CreateUIEditorTab()})
        );
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

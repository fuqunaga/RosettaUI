using System.Linq;
using RosettaUI;
using RosettaUI.Editor.UIToolkit;
using RosettaUI.Example;
using UnityEngine;

public class RosettaUIEditorWindowExample : RosettaUIEditorWindowUIToolkit
{
    [UnityEditor.MenuItem("RosettaUI/RosettaUIEditorWindowExample")]
    public static void ShowExample()
    {
        var wnd = GetWindow<RosettaUIEditorWindowExample>();
        wnd.titleContent = new GUIContent(nameof(RosettaUIEditorWindowExample));
    }

    
    protected override Element CreateElement()
    {
        var idx = 0;
        var types = RosettaUIExample.ExampleTypes;

        return UI.Column(
            UI.Dropdown(null, () => idx, types.Select(t => t.ToString())),
            UI.DynamicElementOnStatusChanged(
                () => idx,
                _ =>
                {
                    var obj = FindObjectOfType(types[idx], true);
                    return UI.Field(() => obj);
                }
            )
        );
    }
}

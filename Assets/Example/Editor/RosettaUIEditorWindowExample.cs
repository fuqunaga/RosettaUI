using RosettaUI;
using RosettaUI.Editor.UIToolkit;
using RosettaUI.Example;
using UnityEngine;

public class RosettaUIEditorWindowExample : RosettaUIEditorWindowUIToolkit
{
    private readonly FieldExample _fieldExample = new();
    
    [UnityEditor.MenuItem("Example/RosettaUITestEditorWindow (UI Toolkit)")]
    public static void ShowExample()
    {
        var wnd = GetWindow<RosettaUIEditorWindowExample>();
        wnd.titleContent = new GUIContent(nameof(RosettaUIEditorWindowExample));
    }

    protected override Element CreateElement()
    {
        return _fieldExample.CreateElement();
    }
}

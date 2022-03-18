using UnityEngine;

namespace RosettaUI.Example
{
    public class UICustomExample : MonoBehaviour, IElementCreator
    {
        public SimpleClass simpleClass = new SimpleClass();
        public Vector2 vector2Value;
        
        public Element CreateElement()
        {
            using var fieldOrProperty = new UICustomScopePropertyOrFields<SimpleClass>("floatValue", "privateValue");
            using var modifier = new UICustomScopePropertyOrFieldLabelModifier<Vector2>(("x", "left"), ("y", "right"));

            return UI.Column(
                UI.Label($"Specify which members will automatically appear in the UI"),
                UI.Indent(
                    UI.Box(
                        UI.Label(
                            "using var _ = new UICustomScopePropertyOrFields<SimpleClass>(\"floatValue\", \"privateValue\");")
                    ),
                    UI.Field(() => simpleClass)
                ),

                UI.Label("Change the label of a specific member"),
                UI.Indent(
                    UI.Box(UI.Label(
                        "using var _ = new UICustomScopePropertyOrFieldLabelModifier<Vector2>((\"x\", \"left\"), (\"y\", \"right\"));")),
                    UI.Field(() => vector2Value)
                )
            );
        }
    }
}
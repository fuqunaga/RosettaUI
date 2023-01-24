using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Editor
{
    public class ObjectFieldCustom : ObjectField
    {
        public ObjectFieldCustom()
        {
            // Hack to pingObject like IMGUI even if Disabled.
            var objectFieldDisplay = this.Q(null, objectUssClassName);
            UIToolkitUtility.RegisterCallbackIncludeDisabled<ClickEvent>(objectFieldDisplay, e =>
            {
                if (objectFieldDisplay.enabledInHierarchy) return;
                
                if (e.clickCount == 1)
                {
                    EditorGUIUtility.PingObject(value);
                }
            });
        }
    }
}
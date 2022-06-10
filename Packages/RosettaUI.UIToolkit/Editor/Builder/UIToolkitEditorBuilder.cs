using RosettaUI.Editor;
using RosettaUI.UIToolkit.Builder;
using RosettaUI.UIToolkit.UnityInternalAccess;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Editor.Builder
{
    [InitializeOnLoad]
    public class UIToolkitEditorBuilder
    {
        static UIToolkitEditorBuilder()
        {
            RegisterBuildFunc();
        }

        private static void RegisterBuildFunc()
        {
            var builder = UIToolkitBuilder.Instance;
            builder.RegisterBuildFunc(typeof(ObjectFieldElement), Build_ObjectField);
        }
        
        private static VisualElement Build_ObjectField(Element e)
        {
            var builder = UIToolkitBuilder.Instance;
            
            var objectFieldElement = (ObjectFieldElement) e;
            var objectField = builder.Build_Field<Object, ObjectField>(objectFieldElement);
            objectField.objectType = objectFieldElement.objectType;

            // Hack to pingObject like IMGUI even if Disabled.
            var objectFieldDisplay = objectField.Q(null, ObjectField.objectUssClassName);
            UIToolkitUtility.RegisterCallbackIncludeDisabled<ClickEvent>(objectFieldDisplay, e =>
            {
                if (!objectFieldDisplay.enabledInHierarchy)
                {
                    if (e.clickCount == 1)
                    {
                        EditorGUIUtility.PingObject(objectField.value);
                    }
                }
            });
            

            return objectField;
        }
    }
}
using RosettaUI.Editor;
using RosettaUI.UIToolkit.Builder;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

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
            builder.RegisterBuildFunc(typeof(ObjectFieldElement), e =>
            {
                var objectFieldElement = (ObjectFieldElement) e;
                var objectField = builder.Build_Field<Object, ObjectField>(objectFieldElement);
                objectField.objectType = objectFieldElement.objectType;

                return objectField;
            });
        }
    }
}
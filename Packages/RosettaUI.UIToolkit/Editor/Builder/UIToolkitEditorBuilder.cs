using RosettaUI.Editor;
using RosettaUI.UIToolkit.Builder;
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
            builder.RegisterBuildBindFunc(typeof(ObjectFieldElement), BuildBindFunc<ObjectField>.Create(Bind_ObjectField));
        }
        
        private static bool Bind_ObjectField(Element element, VisualElement visualElement)
        {
            if (element is not ObjectFieldElement objectFieldElement ||
                visualElement is not ObjectField objectField) return false;
            
            var builder = UIToolkitBuilder.Instance;
            
            builder.Bind_Field<Object, ObjectField>(objectFieldElement, objectField);
            objectField.objectType = objectFieldElement.objectType;

            return true;
        }
    }
}
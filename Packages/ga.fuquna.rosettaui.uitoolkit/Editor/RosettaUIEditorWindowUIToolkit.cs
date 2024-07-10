using RosettaUI.UIToolkit.Builder;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Editor
{
    public abstract class RosettaUIEditorWindowUIToolkit : EditorWindow
    {
        private static StyleSheet _styleSheet;
        private static readonly string StyleSheetPath = "Packages/ga.fuquna.rosettaui.uitoolkit/Runtime/Settings/RosettaUI_DefaultRuntimeTheme.tss";

        public static StyleSheet StyleSheet
        {
            get => _styleSheet ??= AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            set => _styleSheet = value;
        }
        
        
        protected ElementUpdater updater;
        protected Element element;
        
        protected abstract Element CreateElement();

        protected virtual void CreateGUI()
        {
            element = CreateElement();

            var ve = UIToolkitBuilder.Build(element);
            if (ve == null) return;
            
            rootVisualElement.styleSheets.Clear();
            rootVisualElement.styleSheets.Add(StyleSheet);
            
            rootVisualElement.AddToClassList(RosettaUIRootUIToolkit.USSRootClassName);
            rootVisualElement.Add(ve);
            
            updater ??= new ElementUpdater();
            updater.Register(element);
            updater.RegisterWindowRecursive(element);
        }

        protected virtual void Update()
        {
            updater?.Update();
        }

        protected virtual void OnDestroy()
        {
            element.DetachView();
        }
    }
}
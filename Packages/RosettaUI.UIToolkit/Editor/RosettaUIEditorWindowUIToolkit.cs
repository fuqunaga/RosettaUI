using RosettaUI.UIToolkit.Builder;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.Editor.UIToolkit
{
    public abstract class RosettaUIEditorWindowUIToolkit : EditorWindow
    {
        private static StyleSheet _styleSheet;
        private static readonly string StyleSheetPath = "Packages/ga.fuquna.rosettaui.uitoolkit/Settings/RosettaUI_DefaultRuntimeTheme.tss";

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

            var scalingContainer = CreateScalingContainer();
           
            scalingContainer.Add(ve);
            rootVisualElement.Add(scalingContainer);
            
            updater ??= new ElementUpdater();
            updater.Register(element);
            updater.RegisterWindowRecursive(element);
        }


        // PanelSetting like scaling for Editor
        // transform.scale でスケールするが、width、heightをスケール後の見た目が変わらないように調整する
        // またスケールはElementの中心を中心として行われるので左上中心のスケールになるようにする
        // https://forum.unity.com/threads/configuring-panelsettings-used-by-editor-windows.1058072/
        VisualElement CreateScalingContainer()
        {
            const float scale = 12f / 18f;
            
            var scalingContainer = new VisualElement()
            {
                name = "ScalingContainer"
            };
            
            rootVisualElement.RegisterCallback<GeometryChangedEvent>(_ =>
            {
                var rect = rootVisualElement.layout;
                
                var width = rect.width / scale;
                var height = rect.height / scale;

                scalingContainer.style.width = width;
                scalingContainer.style.height = height;
              
                scalingContainer.transform.scale = Vector3.one * scale;
            });
            
            scalingContainer.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                var rect = evt.newRect;
                
                scalingContainer.transform.position = new Vector2(
                    rect.width * (scale - 1f) * 0.5f,
                    rect.height * (scale - 1f) * 0.5f
                );
            });

            return scalingContainer;
        }

        protected virtual void Update()
        {
            updater?.Update();
        }

        protected virtual void OnDestroy()
        {
            element.Destroy();
        }
    }
}
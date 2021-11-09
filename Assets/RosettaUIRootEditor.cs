using RosettaUI.UIToolkit;
using UnityEditor;

namespace RosettaUI.Editors
{
    [CustomEditor(typeof(RosettaUIRootUIToolkit))]
    public class RosettaUIRootEditor : Editor
    {
        private bool _isOpen;
            
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _isOpen = EditorGUILayout.Foldout(_isOpen, "Updater Elements");
            if (_isOpen)
            {
                EditorGUI.indentLevel++;
                
                var root = target as RosettaUIRoot;
                if (root != null)
                {
                    var updater = root.Updater;

                    foreach (var e in updater.Elements)
                    {
                        var label = e.FirstLabel()?.Value ?? "(label not found)";
                        EditorGUILayout.Toggle(label, e.Enable);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
}
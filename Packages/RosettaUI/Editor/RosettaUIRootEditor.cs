using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace RosettaUI.Editors
{
    /// <summary>
    /// Display the Element to be updated in the RosettaUIRoot inspector.
    /// </summary>
    [CustomEditor(typeof(RosettaUIRoot), true)]
    public class RosettaUIRootEditor : Editor
    {
        private bool _isOpen;

        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (_isOpen)
            {
                UpdateDatas();
            }
        }


        private List<(string, bool)> _updateDataList = new List<(string, bool)>();
        private void UpdateDatas()
        {
            var root = target as RosettaUIRoot;
            if (root != null)
            {
                var newDataList = root.updater.Elements.Select(e => (e.FirstLabel()?.Value, e.Enable)).ToList();
                var changed = (_updateDataList.Count != newDataList.Count)
                              || _updateDataList.Zip(newDataList, (d0, d1) => d0 != d1).Any(diff => diff);

                if (changed)
                {
                    _updateDataList = newDataList;
                    Repaint();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _isOpen = EditorGUILayout.Foldout(_isOpen, "Updater Elements");
            if (_isOpen)
            {
                EditorGUI.indentLevel++;

                foreach (var (labelString, enable) in _updateDataList)
                {
                    var label = labelString ?? "(label not found)";
                    EditorGUILayout.Toggle(label, enable);
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using RosettaUI.Undo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.Editor.UndoSystem
{
    public class UndoHistoryEditorWindow : EditorWindow
    {
        [UnityEditor.MenuItem("Window/RosettaUI/Undo History")]
        public static void Open()
        {
            var window = GetWindow<UndoHistoryEditorWindow>();
            window.titleContent = new GUIContent("Undo History");
            window.Show();
        }
        
        private MultiColumnListView _multiColumnListView;
        
        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        private void CreateGUI()
        {
            _multiColumnListView  = new MultiColumnListView()
            {
                selectionType = SelectionType.None,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly
            };

            var nameColumn = new Column()
            {
                name = nameof(IUndoRecord.Name),
                title = nameof(IUndoRecord.Name),
                bindCell = BindCellName,
                width = 200,
            };
            
            var expiredColumn = new Column()
            {
                name = nameof(IUndoRecord.IsExpired),
                title = nameof(IUndoRecord.IsExpired),
                makeCell = () =>
                {
                    var container = new VisualElement(){ style = { justifyContent = Justify.Center } };
                    container.Add(new Toggle());
                    return container;
                },
                bindCell = BindCellExpired,
                width = 100,
            };
            
            _multiColumnListView.columns.Add(nameColumn);
            _multiColumnListView.columns.Add(expiredColumn);
          
            rootVisualElement.Add(_multiColumnListView );

            return;
            
            void BindCellName(VisualElement element, int i)
            {
                var list = (List<IUndoRecord>)_multiColumnListView.itemsSource;
                ((Label)element).text = list[i].Name;
                SetCellStyle(element, i);
            }

            void BindCellExpired(VisualElement element, int i)
            {
                var list = (List<IUndoRecord>)_multiColumnListView.itemsSource;
                var toggle = element.Q<Toggle>();
                toggle.value = list[i].IsExpired;
                toggle.style.justifyContent = Justify.Center;
                SetCellStyle(toggle, i);
            }

            void SetCellStyle(VisualElement element, int i)
            {
                var redoRecordCount = UndoHistory.RedoRecords.Count();
                // ReSharper disable once AccessToModifiedClosure
                var opacity = i < redoRecordCount ? 0.5f : 1f;
                element.style.opacity = opacity;
            }
        }

        private void OnEditorUpdate()
        {
            var list = UndoHistory.RedoRecords.Reverse().Concat(UndoHistory.UndoRecords).ToList();
            _multiColumnListView.itemsSource = list;
        }
    }
}
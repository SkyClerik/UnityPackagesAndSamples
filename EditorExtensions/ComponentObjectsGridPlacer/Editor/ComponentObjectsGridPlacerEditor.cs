using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Helper
{
    [CustomEditor(typeof(EditorComponentObjectsGridPlacer))]
    public class ComponentObjectsGridPlacerEditor : Editor
    {
        private EditorComponentObjectsGridPlacer _target;

        private void OnEnable()
        {
            _target = target as EditorComponentObjectsGridPlacer;
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var newButton = CreateButton("Собрать детей в лист", () =>
            {
                _target.AddChildrenToList();
                EditorUtility.SetDirty(_target);
            });
            root.Add(newButton);

            var newButton_1 = CreateButton("Выставить по сетке", () =>
            {
                _target.SetPlacer();
                EditorUtility.SetDirty(_target);
            });
            root.Add(newButton_1);

            return root;
        }

        private Button CreateButton(string text, System.Action callback)
        {
            Button element = new Button();
            element.text = text;
            element.clicked += callback;
            return element;
        }
    }
}
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik
{
    [CustomEditor(typeof(EditorComponentStateHandler))]
    public class ComponentStateHandlerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            // стандартный инспектор (components + stateAssets)
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            // берём SerializedProperty для списка SO
            var stateAssetsProp = serializedObject.FindProperty("stateAssets");

            if (stateAssetsProp != null)
            {
                var handler = (EditorComponentStateHandler)target;

                var block = new VisualElement();
                block.style.marginTop = 6;
                block.Add(new Label("Сохранение/загрузка по ассетам:"));

                for (int i = 0; i < stateAssetsProp.arraySize; i++)
                {
                    var elementProp = stateAssetsProp.GetArrayElementAtIndex(i);
                    var asset = elementProp.objectReferenceValue as ComponentsStateSO;
                    if (asset == null)
                        continue;

                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.marginTop = 2;
                    row.style.marginBottom = 2;

                    var label = new Label(asset.name);
                    label.style.minWidth = 120;
                    label.style.unityTextAlign = TextAnchor.MiddleLeft;

                    var saveBtn = new Button(() =>
                    {
                        handler.SaveAllTo(asset);
                    })
                    {
                        text = $"Сохранить {asset.name}"
                    };

                    var loadBtn = new Button(() =>
                    {
                        handler.LoadAllFrom(asset);
                    })
                    {
                        text = $"Загрузить {asset.name}"
                    };

                    row.Add(label);
                    row.Add(saveBtn);
                    row.Add(loadBtn);

                    block.Add(row);
                }

                root.Add(block);
            }

            return root;
        }
    }
}

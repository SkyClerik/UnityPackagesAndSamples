#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(EditorComponentCreatePrimitiveGrid))]
public class ComponentCreatePrimitiveGridEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();

        // Стандартные поля
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.Add(new VisualElement { style = { height = 4 } });

        var generateButton = new Button(() =>
        {
            var grid = (EditorComponentCreatePrimitiveGrid)target;
            grid.GenerateGrid();
            EditorUtility.SetDirty(grid);
        })
        { text = "Generate Grid" };

        var clearButton = new Button(() =>
        {
            var grid = (EditorComponentCreatePrimitiveGrid)target;
            grid.ClearGrid();
            EditorUtility.SetDirty(grid);
        })
        { text = "Clear Grid" };

        root.Add(generateButton);
        root.Add(clearButton);

        return root;
    }
}
#endif
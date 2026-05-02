using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace SkyClerik.CustomEditorExtensions
{
    [InitializeOnLoad]
    public static class OfficeKeyContextMenu
    {
        static OfficeKeyContextMenu()
        {
            FieldInfo info = typeof(EditorApplication).GetField("globalEventHandler",
                BindingFlags.Static | BindingFlags.NonPublic);

            EditorApplication.CallbackFunction callback = (EditorApplication.CallbackFunction)info.GetValue(null);
            callback += OnKeyEvent;
            info.SetValue(null, callback);
        }

        private static void OnKeyEvent()
        {
            Event e = Event.current;
            if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.Menu)
            {
                if (Selection.activeObject != null)
                {
                    // Если выделен объект на сцене (у него есть путь в иерархии)
                    bool isSceneObject = Selection.activeGameObject != null && !AssetDatabase.Contains(Selection.activeGameObject);

                    // Выбираем путь меню в зависимости от того, где фокус
                    string menuPath = isSceneObject ? "GameObject/" : "Assets/";

                    EditorUtility.DisplayPopupMenu(new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0), menuPath, null);
                    e.Use();
                }
            }
        }
    }
}

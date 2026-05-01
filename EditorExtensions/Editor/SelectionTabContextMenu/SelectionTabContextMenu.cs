using UnityEditor;

namespace SkyClerik.Develop
{
    public static class SelectionTabContextMenu
    {
        [MenuItem("Assets/Selection Tab Window %#q")] // Ctrl+Shift+T (Windows) / Cmd+Shift+T (macOS):
        private static void OpenSelectionTab()
        {
            SelectionTabWindow.ShowWithSelection();
        }
    }
}
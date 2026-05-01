using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Develop
{
    public class ExportSelectedPrefabsService : ISelectionTabService
    {
        public string Title => "Export Selected Prefabs";
        public string Description => "Экспорт выделенных префабов с зависимостями в .unitypackage";
        public Texture2D Icon => null;

        public bool IsAvailable(Object[] selection)
        {
            if (selection == null || selection.Length == 0)
                return false;

            // Есть ли среди выделенных хотя бы один prefab-ассет в Project
            return GetPrefabAssetPaths(selection).Any();
        }

        public void Execute(SelectionTabWindow window)
        {
            var selection = Selection.objects;
            var prefabPaths = GetPrefabAssetPaths(selection).ToList();

            if (prefabPaths.Count == 0)
            {
                Debug.LogWarning("ExportSelectedPrefabsService: среди выделенных нет префабов в Project.");
                window.Close();
                return;
            }

            // Собираем все файлы для экспорта
            var filesToExport = CollectDependencies(prefabPaths);

            // Окно выбора пути
            string defaultName = prefabPaths.Count == 1
                ? System.IO.Path.GetFileNameWithoutExtension(prefabPaths[0]) + ".unitypackage"
                : "PrefabsExport.unitypackage";

            string exportPath = EditorUtility.SaveFilePanel("Export Unity Package", "", defaultName, "unitypackage");
            if (string.IsNullOrEmpty(exportPath))
            {
                // отмена — просто закрываем таб
                window.Close();
                return;
            }

            AssetDatabase.ExportPackage(filesToExport.ToArray(), exportPath, ExportPackageOptions.Interactive);
            Debug.Log($"ExportSelectedPrefabsService: exported {filesToExport.Count} files to {exportPath}");

            window.Close();
        }

        // --- helpers ---

        private IEnumerable<string> GetPrefabAssetPaths(Object[] selection)
        {
            foreach (var obj in selection)
            {
                if (obj == null)
                    continue;

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                // фильтруем только prefab-ассеты в Project
                if (type == typeof(GameObject))
                    yield return path;
            }
        }

        private List<string> CollectDependencies(IEnumerable<string> prefabPaths)
        {
            var filesToExport = new HashSet<string>();

            foreach (var prefabPath in prefabPaths)
            {
                // все зависимости префаба
                var deps = AssetDatabase.GetDependencies(prefabPath, true);
                foreach (var dep in deps)
                    filesToExport.Add(dep);

                // + скрипты компонентов (на всякий случай, как в твоём исходном коде)
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                    EnsureScriptDependencies(prefab, filesToExport);
            }

            return filesToExport.ToList();
        }

        private void EnsureScriptDependencies(GameObject prefab, HashSet<string> filesToExport)
        {
            var components = prefab.GetComponentsInChildren<Component>(true);
            foreach (var component in components)
            {
                if (component == null)
                    continue;

                if (component is MonoBehaviour mb)
                {
                    var mono = MonoScript.FromMonoBehaviour(mb);
                    if (mono == null)
                        continue;

                    string scriptPath = AssetDatabase.GetAssetPath(mono);
                    if (!string.IsNullOrEmpty(scriptPath))
                        filesToExport.Add(scriptPath);
                }
            }
        }
    }
}
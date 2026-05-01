using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Develop
{
    public class ExampleSelectionTabService : ISelectionTabService
    {
        public string Title => "Example Service";
        public string Description => "Example Description";
        private Texture2D _icon;
        public Texture2D Icon
        {
            get
            {
                if (_icon == null)
                    _icon = EditorStartupRunner.FindIconByName("sc_editor_image");
                return _icon;
            }
        }

        public bool IsAvailable(Object[] selection)
        {
            // фильтруем только объекты, у которых есть путь в AssetDatabase
            var assetSelection = selection
                .Where(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)))
                .ToArray();

            if (assetSelection.Length == 0)
                return false; // значит, выделены чисто сценовые объекты

            return true;


            // пример: доступен только если есть хоть один объект
            //var available = selection != null && selection.Length > 0;
        }

        public void Execute(SelectionTabWindow window)
        {
            Debug.Log("ExampleSelectionTabService.Execute()");
            window.Close();
        }
    }
}
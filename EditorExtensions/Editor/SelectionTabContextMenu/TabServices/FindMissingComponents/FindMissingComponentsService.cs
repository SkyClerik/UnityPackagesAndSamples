using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SkyClerik.Develop
{
    public class FindMissingComponentsService : ISelectionTabService
    {
        public string Title => "Find Missing Components";
        public string Description => "Находит и выделяет объекты сцены с missing components.";

        private Texture2D _icon;
        public Texture2D Icon
        {
            get
            {
                if (_icon == null)
                    _icon = EditorStartupRunner.FindIconByName("sc_editor_lost");
                return _icon;
            }
        }

        public bool IsAvailable(Object[] selection)
        {
            // Логика сервиса не зависит от выделения — можно всегда показывать кнопку,
            // или, если хочешь, только когда есть открытая сцена.
            return true;
        }

        public void Execute(SelectionTabWindow window)
        {
            var offenders = new List<GameObject>();

            // берём все Transform в активной сцене (включая неактивные)
            Transform[] allTransformsInScene = GameObject.FindObjectsOfType<Transform>(true);

            foreach (var t in allTransformsInScene)
            {
                var go = t.gameObject;
                var comps = go.GetComponents<Component>();
                foreach (var c in comps)
                {
                    if (c == null)
                    {
                        offenders.Add(go);
                        break;
                    }
                }
            }

            if (offenders.Count > 0)
            {
                var distinct = offenders.Distinct().ToArray();
                Selection.objects = distinct;

                var details = string.Join("\n", distinct.Select(go => go.name));
                EditorUtility.DisplayDialog(
                    "Missing Components Found",
                    $"Найдено объектов: {distinct.Length}\n\n{details}",
                    "OK"
                );
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Missing Components",
                    "В текущей сцене нет объектов с missing components.",
                    "OK"
                );
            }

            window.Close();
            }
        }
    }
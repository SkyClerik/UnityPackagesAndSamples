using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Develop
{
    [Overlay(typeof(SceneView), "ObjectsCollectionToolkit")]
    public class ObjectsCollectionToolkit : Overlay, ICreateToolbar
    {
        static readonly string[] k_ToolbarItems = new[]
       {
        "MyToolbarItem",
        //"SomeOtherToolbarItem"
        };
        private ObjectsCollectionToolkitDefinition _data;
        public IEnumerable<string> toolbarElements => k_ToolbarItems;


        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            if (EditorStartupRunner.TryGetData(out _data))
            {
                var applicationDescription = new Label($"Description: {_data.ApplicationDescription}");
                var goToSettings = new Button();
                goToSettings.text = "Выделить настройки";
                goToSettings.clicked += GoToSettings_clicked;
                root.Add(goToSettings);
            }

            return root;
        }

        private void GoToSettings_clicked()
        {
            PingObject(_data);
        }

        private void PingObject(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj); // Выделить объект в проекте
                Selection.activeObject = obj; // Дополнительно выделяем в инспекторе
            }
        }
    }

    [EditorToolbarElement("MyToolbarItem", typeof(SceneView))]
    class MyToolbarItem : OverlayToolbar
    {
        private ObjectsCollectionToolkitDefinition _data;

        public MyToolbarItem()
        {
            if (EditorStartupRunner.TryGetData(out _data))
            {
                var CollectionButton = new EditorToolbarButton()
                {
                    text = "Collection",
                    tooltip = "Собрать объекты в коллекцию",
                    icon = _data.IconCollection,
                };
                CollectionButton.clicked += Collection;
                Add(CollectionButton);

                var SelectedRootButton = new EditorToolbarButton()
                {
                    text = "SelectRoot",
                    tooltip = "Выделить коллекцию",
                    icon = _data.IconSelectRoot,
                };
                SelectedRootButton.clicked += SelectedRoot;
                Add(SelectedRootButton);

                var MergeRootsButton = new EditorToolbarButton()
                {
                    text = "MergeRoots",
                    tooltip = "Объединить в одну коллекцию",
                    icon = _data.IconMerge,
                };
                MergeRootsButton.clicked += MergeRoots;
                Add(MergeRootsButton);

                var ClearParentButton = new EditorToolbarButton()
                {
                    text = "ClearParent",
                    tooltip = "Вынести объект из коллекции сохранив позицию",
                    icon = _data.IconClearParent,
                };
                ClearParentButton.clicked += ClearParent;
                Add(ClearParentButton);

                var RotateRightButton = new EditorToolbarButton()
                {
                    text = "RotateRight",
                    tooltip = "Повернуть объект на 90 градусов",
                    icon = _data.IconRotate,
                };
                RotateRightButton.clicked += RotateRight;
                Add(RotateRightButton);

                var RoundToNearestQuarterButton = new EditorToolbarButton()
                {
                    text = "RoundTo",
                    tooltip = $"Округлить координаты объекта с шагом {_data.RoundStep}",
                    icon = _data.IconRoundItUp,
                };
                RoundToNearestQuarterButton.clicked += RoundToNearestQuarter;
                Add(RoundToNearestQuarterButton);

                var GridPlaceButton = new EditorToolbarButton()
                {
                    text = "Grid",
                    tooltip = "Расположить выделенные объекты по сетке",
                    icon = _data.IconGreed,
                };
                GridPlaceButton.clicked += PlaceSelectionOnGrid;
                Add(GridPlaceButton);

            }
        }

        private void SelectedRoot()
        {
            var selectedObjects = Selection.transforms;

            if (selectedObjects.Length > 0)
            {
                if (selectedObjects[0].parent != null)
                {
                    Selection.activeObject = selectedObjects[0].parent;
                }
            }
        }

        private void RotateRight()
        {
            Transform[] selectedObjects = Selection.transforms;

            Undo.RecordObjects(selectedObjects, "RotateRight");
            for (int i = 0; i < selectedObjects.Length; i++)
            {
                var angle = selectedObjects[i].localEulerAngles.y + _data.RotateAnge >= 360 ? 0 : Mathf.RoundToInt(selectedObjects[i].localEulerAngles.y + _data.RotateAnge);
                selectedObjects[i].rotation = Quaternion.Euler(selectedObjects[i].localEulerAngles.x, angle, selectedObjects[i].localEulerAngles.z);
            }
        }

        public void RoundToNearestQuarter()
        {
            Transform[] selectedObjects = Selection.transforms;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("Нет выделенных объектов для округления координат.");
                return;
            }

            // получаем делитель из enum: Quarter => 4, Half => 2 и т.п.
            float divider = (float)_data.RoundStep; // enum приведётся к int, потом к float
            if (divider <= 0f)
            {
                Debug.LogWarning("Некорректный шаг округления (divider <= 0). Проверь настройку в SuperCollectionData.");
                return;
            }

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                var t = selectedObjects[i];
                if (t == null) continue;

                Undo.SetTransformParent(t, t.parent, true, "RoundToGridStep");

                var pos = t.position;
                pos.x = Mathf.Round(pos.x * divider) / divider;
                pos.y = Mathf.Round(pos.y * divider) / divider;
                pos.z = Mathf.Round(pos.z * divider) / divider;
                t.position = pos;
            }
        }


        private void MergeRoots()
        {
            Transform[] selectedObjects = Selection.transforms;
            List<Transform> whiteList = new List<Transform>();

            for (int i = 0; i < selectedObjects.Length; i++)
            {
                if (selectedObjects[i].parent != null)
                    whiteList.Add(selectedObjects[i].parent);
            }

            whiteList = whiteList.Distinct().ToList();
            Transform lastParent = whiteList.Last();

            Undo.SetTransformParent(lastParent, null, true, "Set new parent");
            lastParent.SetParent(null, true);

            if (whiteList.Count >= 2)
            {
                for (int i = 0; i < whiteList.Count; i++)
                {
                    var childrens = whiteList[i].transform.GetComponentsInChildren<Transform>();
                    for (int j = 0; j < childrens.Length; j++)
                    {
                        Undo.SetTransformParent(childrens[j], lastParent.transform, true, "Set new parent");
                        childrens[j].SetParent(lastParent.transform, true);
                    }
                }

                for (int i = 0; i < whiteList.Count; i++)
                {
                    if (lastParent == whiteList[i])
                        continue;

                    Undo.DestroyObjectImmediate(whiteList[i]);
                    Object.DestroyImmediate(whiteList[i]);
                }
            }
            else
            {
                Debug.LogWarning($"Для объединения требуется несколько колекций");
            }

            Selection.activeObject = lastParent.gameObject;
            SelectedRoot();
        }

        private void ClearParent()
        {
            Transform[] selectedObjects = Selection.transforms;
            if (selectedObjects.Length > 0)
            {
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    Undo.SetTransformParent(selectedObjects[i], null, true, "Set new parent");
                    selectedObjects[i].SetParent(null, true);
                }

                Selection.activeObject = selectedObjects[0];
                SelectedRoot();
            }
        }

        private void Collection()
        {
            var selectedObjects = Selection.transforms;

            if (selectedObjects.Length > 0)
            {
                var empty = CreateEmptyObject();
                var last = selectedObjects[selectedObjects.Length - 1];
                empty.transform.position = last.transform.position;
                empty.name = $"{last.name}";

                foreach (var obj in selectedObjects)
                {
                    Undo.SetTransformParent(obj, empty.transform, true, "Set new parent");
                    obj.transform.SetParent(empty.transform, true);
                }

                SelectedRoot();
            }
        }

        private GameObject CreateEmptyObject()
        {
            var emptyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(emptyObject.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(emptyObject.GetComponent<MeshFilter>());
            Object.DestroyImmediate(emptyObject.GetComponent<SphereCollider>());
            Undo.RegisterCreatedObjectUndo(emptyObject, "Create emptyObject");
            return emptyObject;
        }

        /// <summary>
        /// Раскладывает текущие выделенные объекты по сетке в мировых координатах.
        /// </summary>
        private void PlaceSelectionOnGrid()
        {
            Transform[] selectedObjects = Selection.transforms;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                Debug.LogWarning("Нет выделенных объектов для размещения по сетке.");
                return;
            }

            // Регистрируем операцию для Undo
            Undo.RecordObjects(selectedObjects, "Place Selection On Grid");

            Vector2 offset = _data.GridOffset;
            int stack = 0;

            foreach (Transform obj in selectedObjects)
            {
                if (obj == null) continue;

                obj.position = new Vector3(offset.x, 0f, offset.y);

                stack++;
                offset.x += _data.GridOffset.x;

                // перенос на следующую "строку"
                if (stack >= _data.GridStack)
                {
                    offset.x = _data.GridOffset.x;
                    offset.y += _data.GridOffset.y;
                    stack = 0;
                }
            }
        }
    }

    [EditorToolbarElement("SomeOtherToolbarItem", typeof(SceneView))]
    class SomeOtherToolbarItem : EditorToolbarToggle
    {
        public SomeOtherToolbarItem()
        {
            icon = EditorGUIUtility.FindTexture("CustomTool");
        }
    }
}
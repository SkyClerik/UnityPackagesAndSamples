using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Develop
{
    public class SelectionTabWindow : EditorWindow
    {
        private readonly List<ISelectionTabService> _services = new List<ISelectionTabService>();

        public static void ShowWithSelection()
        {
            var window = GetWindow<SelectionTabWindow>();
            window.titleContent = new GUIContent("Selection Tab");
            window.SetupInitialRect();
            window.Show();
            window.Focus();
        }

        private void SetupInitialRect()
        {
            var screen = Screen.currentResolution;
            float w = 250;
            float h = 300;

            float x = (screen.width - w) * 0.5f;
            float y = (screen.height - h) * 0.5f;

            position = new Rect(x, y, w, h);
            minSize = new Vector2(w, h);
        }


        private void OnLostFocus()
        {
            //Close();
        }

        private void RegisterDefaultServices()
        {
            _services.Clear();
            _services.Add(new ExampleSelectionTabService());
            _services.Add(new ExportSelectedPrefabsService());
            _services.Add(new ApplyMeshToNewFbxService());
            _services.Add(new FindMissingComponentsService());
        }

        public void CreateGUI()
        {
            // === регаем сервисы перед построением UI ===
            RegisterDefaultServices();

            if (_services.Count == 0)
                Close();

            var root = rootVisualElement;
            root.style.paddingLeft = 6;
            root.style.paddingRight = 6;
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;

            // === обработка ESC ===

            root.RegisterCallback<KeyDownEvent>(OnKeyDownEvent, TrickleDown.TrickleDown);

            // === Горизонтальный скроллер для кнопок сервисов ===

            var scroll = new ScrollView(ScrollViewMode.Horizontal);
            scroll.style.minHeight = 30;
            scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
            scroll.style.width = Length.Percent(100);
            scroll.style.height = Length.Percent(100);
            scroll.style.alignItems = Align.Center;
            scroll.style.justifyContent = Justify.Center;
            root.Add(scroll);

            // === Контейнер с кнопками внутри скролла ===

            var buttonsRow = new VisualElement();
            buttonsRow.style.paddingLeft = 6;
            buttonsRow.style.paddingRight = 6;
            buttonsRow.style.paddingTop = 6;
            buttonsRow.style.paddingBottom = 6;
            buttonsRow.style.flexGrow = 1;
            buttonsRow.style.justifyContent = Justify.Center;
            scroll.Add(buttonsRow);

            // === ловим смену фокуса на потомках ===
            buttonsRow.RegisterCallback<FocusInEvent>(OnButtonFocusIn, TrickleDown.TrickleDown);

            // === Подключение всех сервисов ===

            foreach (var service in _services)
            {
                if (service == null || !service.IsAvailable(Selection.objects))
                    continue;

                var capturedService = service; // чтобы замкнуть корректно
                var toolbarButton = new EditorToolbarButton(() => capturedService.Execute(this))
                {
                    text = capturedService.Title,
                    tooltip = capturedService.Description,
                    icon = service.Icon != null ? service.Icon : EditorStartupRunner.FindIconByName("sc_editor_image"),
                };
                toolbarButton[0].style.maxWidth = 30;
                toolbarButton[0].style.maxWidth = 30;
                toolbarButton.style.flexDirection = FlexDirection.Row;
                toolbarButton.style.alignItems = Align.Center;
                toolbarButton.style.justifyContent = Justify.SpaceBetween;
                toolbarButton.style.maxHeight = 30;
                toolbarButton.style.minHeight = 30;
                toolbarButton.style.minWidth = 100;
                toolbarButton.style.paddingLeft = 2;
                toolbarButton.style.paddingRight = 2;
                toolbarButton.style.paddingTop = 2;
                toolbarButton.style.paddingBottom = 2;
                toolbarButton.style.borderTopWidth = 2;
                toolbarButton.style.borderLeftWidth = 2;
                toolbarButton.style.borderRightWidth = 2;
                toolbarButton.style.borderBottomWidth = 2;
                toolbarButton.style.borderBottomLeftRadius = 6;
                toolbarButton.style.borderBottomRightRadius = 6;
                toolbarButton.style.borderTopLeftRadius = 6;
                toolbarButton.style.borderTopRightRadius = 6;
                buttonsRow.Add(toolbarButton);
            }
            // === Фокус на первой кнопке ===

            if (buttonsRow.childCount > 0)
            {
                var firstChild = buttonsRow.ElementAt(0);
                firstChild.focusable = true;
                firstChild.Focus();
            }
        }

        private void OnKeyDownEvent(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape)
            {
                Close();
                evt.StopImmediatePropagation();
            }
        }

        private void OnButtonFocusIn(FocusInEvent evt)
        {
            if (evt.target is Button btn)
            {
                Debug.Log($"Focus on button: {btn.text}");

                // если нужен индекс в контейнере:
                var parent = btn.parent as VisualElement;
                if (parent != null)
                {
                    int index = parent.IndexOf(btn);
                    Debug.Log($"Button index: {index}");
                }
            }
        }
    }
}
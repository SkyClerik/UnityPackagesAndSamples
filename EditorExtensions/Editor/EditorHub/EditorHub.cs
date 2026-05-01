using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Develop
{
    public struct EditorHubNodeData
    {
        public string Id;          // для поиска/удаления
        public string ButtonTitle;
        public string Description;
        public string Group;
        public Color IconColor;
        public Texture2D Icon;
        public Action Action;
        public Action<object> ActionWithArg; // небезопасный, универсальный аргумент
    }

    public static class EditorHubRegistry
    {
        static readonly List<EditorHubNodeData> _nodes = new List<EditorHubNodeData>();

        public static IReadOnlyList<EditorHubNodeData> Nodes => _nodes;

        public static void Register(EditorHubNodeData node)
        {
            if (node.Action == null && node.ActionWithArg == null)
                return;

            _nodes.Add(node);
        }

        public static void Clear()
        {
            _nodes.Clear();
        }
    }

    public class EditorHubWindow : EditorWindow
    {
        private bool _onlyIcon;
        private Toggle _onlyIconToggle;
        private VisualElement _buttonArea;
        private float _buttonBoxSize = 20;


        [MenuItem("SkyClerik/Hub")]
        public static void ShowEditorHubWindowWindow()
        {
            var wnd = GetWindow<EditorHubWindow>("SkyClerik Hub");
            wnd.position = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);
            wnd.minSize = new Vector2(32f, 128f);
            wnd.Show();
        }

        // Метод вызывается Unity при создании/открытии окна.
        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            _onlyIconToggle = new Toggle();
            _onlyIconToggle.text = "Only Icon";
            _onlyIconToggle.value = _onlyIcon;
            root.Add(_onlyIconToggle);

            _buttonArea = new ScrollView();
            root.Add(_buttonArea);
            _onlyIconToggle.RegisterValueChangedCallback(OnToggleChanged);

            CreateButtonGroup();
        }

        private void OnDisable()
        {
            if (_onlyIconToggle != null)
                _onlyIconToggle.UnregisterValueChangedCallback(OnToggleChanged);
        }

        private void OnToggleChanged(ChangeEvent<bool> evt)
        {
            _onlyIcon = evt.newValue;
            CreateButtonGroup();
        }

        private void CreateButtonGroup()
        {
            _buttonArea.Clear();

            foreach (var node in EditorHubRegistry.Nodes)
            {
                var nodeElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        marginBottom = 4
                    }
                };

                var buttonElement = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                         alignItems = Align.Center,
                    }
                };
                nodeElement.Add(buttonElement);


                if (!_onlyIcon)
                {
                    var img = CreateImage(node.Icon, node.IconColor);
                    var btnImg = new Button(() => node.Action?.Invoke());
                    btnImg.text = string.Empty;
                    btnImg.Add(img);
                    btnImg.style.alignItems = Align.Center;
                    btnImg.style.alignContent = Align.Center;
                    btnImg.style.alignSelf = Align.Center;
                    btnImg.style.justifyContent = Justify.Center;
                    btnImg.style.width = _buttonBoxSize;
                    btnImg.style.height = _buttonBoxSize;
                    buttonElement.Add(btnImg);

                    var btn = new Button(() => node.Action?.Invoke());
                    btn.text = node.ButtonTitle;
                    btn.style.minHeight = _buttonBoxSize;

                    if (!string.IsNullOrEmpty(node.Description))
                        btn.tooltip = node.Description;

                    buttonElement.Add(btn);
                }
                else // если режим "только иконка"
                {
                    var img = CreateImage(node.Icon, node.IconColor);
                    var btn = new Button(() => node.Action?.Invoke());
                    btn.text = string.Empty;
                    btn.Add(img);
                    btn.style.alignItems = Align.Center;
                    btn.style.alignContent = Align.Center;
                    btn.style.alignSelf = Align.Center;
                    btn.style.justifyContent = Justify.Center;
                    btn.style.width = _buttonBoxSize;
                    btn.style.height = _buttonBoxSize;

                    if (!string.IsNullOrEmpty(node.Description))
                        btn.tooltip = node.Description;

                    buttonElement.Add(btn);
                }

                Foldout group = _buttonArea.Q<Foldout>(node.Group.ToLower());
                if (group == null)
                {
                    group = new Foldout();
                    group.value = false;
                    group.text = node.Group.ToUpper();
                    group.name = node.Group.ToLower();
                    _buttonArea.Add(group);
                }

                group.Add(nodeElement);
            }
        }

        private Image CreateImage(Texture2D texture2D, Color color)
        {
            if (texture2D == null)
                texture2D = EditorStartupRunner.FindIconByName("sc_editor_image");

            if (color == new Color(0, 0, 0, 0))
                color = new Color(0.7686275f, 0.7686275f, 0.7686275f, 1); // стандартный серый цвет элементов редактора

            var img = new Image();
            img.style.backgroundImage = new StyleBackground(texture2D);
            img.style.unityBackgroundImageTintColor = color;
            img.style.width = _buttonBoxSize - 4;
            img.style.height = _buttonBoxSize - 4;
            img.style.flexShrink = 0;
            return img;
        }
    }
}

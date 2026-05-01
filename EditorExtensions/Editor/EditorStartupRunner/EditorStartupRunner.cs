//#define LOG
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkyClerik.Develop
{
    public interface IEditorStartupPlugin
    {
        void OnEditorStartup();
    }

    [InitializeOnLoad]
    public static class EditorStartupRunner
    {
        // Заменяем обычное поле на ключ для SessionState
        private const string SessionKey = "EditorStartupRunner_AlreadyRun";
        //private static bool _initialized;

        private static bool _isInitializationPending;
        private static double _initializationStartTime;
        private const float DelayDuration = 2f;
        public static EditorStartupDefinition EditorStartup;

        static EditorStartupRunner()
        {
            // Проверяем, было ли уже выполнение в этой сессии
            if (SessionState.GetBool(SessionKey, false))
                return;

            ScheduleInitialize();
        }

        /// <summary>
        /// Планирует отложенную инициализацию, чтобы не цепляться за редактор в момент его активной загрузки.
        /// </summary>
        private static void ScheduleInitialize()
        {
            if (_isInitializationPending)
                return;

            //if (_isInitializationPending || _initialized)
            //    return;

            _isInitializationPending = true;
            _initializationStartTime = EditorApplication.timeSinceStartup;

            // Подписываемся на EditorApplication.update для проверки времени
            EditorApplication.update += DelayedInitializeUpdate;
        }

        /// <summary>
        /// Проверяет, прошла ли задержка, и запускает фактическую инициализацию.
        /// </summary>
        private static void DelayedInitializeUpdate()
        {
            if (EditorApplication.timeSinceStartup - _initializationStartTime < DelayDuration)
                return;

            EditorApplication.update -= DelayedInitializeUpdate;
            _isInitializationPending = false;
            Initialize();
        }

        /// <summary>
        /// Фактическая инициализация.
        /// </summary>
        private static void Initialize()
        {
            // Еще раз проверяем на всякий случай и записываем флаг
            if (SessionState.GetBool(SessionKey, false))
                return;

            // Помечаем, что запуск состоялся. Теперь до перезапуска Unity 
            // этот код не выполнится даже после компиляции.
            SessionState.SetBool(SessionKey, true);

            //if (_initialized)
            //    return;

            //_initialized = true;

            // Пример: подписка на события редактора.
            // EditorApplication.hierarchyChanged += OnHierarchyChanged;
            // EditorApplication.projectChanged += OnProjectChanged;

            Run();
        }

        /// <summary>
        /// Единая точка входа логики после инициализации редактора.
        /// </summary>
        public static void Run()
        {
            if (TryLoadDataForCurrentUser(out EditorStartupRunner.EditorStartup))
            {
                #region Уведомляем плагины
                var pluginType = typeof(IEditorStartupPlugin);
                var plugins = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => pluginType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                foreach (var t in plugins)
                {
                    try
                    {
                        var plugin = (IEditorStartupPlugin)Activator.CreateInstance(t);
                        plugin.OnEditorStartup();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"EditorStartupRunner: не удалось создать {t.Name}: {ex}");
                    }
                }
                #endregion Уведомляем плагины

                #region Работаем с окнами

                // Пример: открыть или обновить своё окно на UI Toolkit
                if (EditorStartupRunner.EditorStartup.ShowStartingWindow)
                    OpenOrCreateWindow();

                #endregion Работаем с окнами
            }
        }

        /// <summary>
        /// Открывает/создаёт кастомное окно редактора на UI Toolkit.
        /// Показывает пример работы с VisualElement.
        /// </summary>
        private static void OpenOrCreateWindow()
        {
            var window = EditorWindow.GetWindow<EditorStartupWindow>();
            window.titleContent = new GUIContent("Startup Tool");
            window.Show();
        }


        /// <summary>
        /// Попробуем найти первыйв попавшийся ассет желаемого типа. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool TryGetData<T>(out T data) where T : ScriptableObject
        {
            data = null;
            string typeName = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets($"t:{typeName}");

            var foundDatas = new List<T>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    foundDatas.Add(asset);
                }
            }

            data = foundDatas.FirstOrDefault();

            if (data != null)
            {
#if LOG
                Debug.Log($"Загружен актив {typeName}");
#endif
                return true;
            }
            else
            {
#if LOG
                Debug.LogWarning($"Актив {typeName} не найден.");
#endif
                return false;
            }
        }

        /// <summary>
        /// Попробуем найти все возможные ассеты желаемого типа в проекте. 
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static bool TryGetDatas<T>(out List<T> datas) where T : ScriptableObject
        {
            datas = new List<T>();
            string typeName = typeof(T).Name;
            string[] guids = AssetDatabase.FindAssets($"t:{typeName}");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    datas.Add(asset);
                }
            }

            if (datas.Count != 0)
            {
#if LOG
                var log = "Список найденных активов : ";
                foreach (var fd in datas)
                {
                log += $"{fd.name}, ";
                }
                Debug.Log(log);
#endif
                return true;
            }
            else
            {
#if LOG
                Debug.LogWarning($"Активы типа '{typeName}' не найдены.");
#endif
                return false;
            }
        }

        private static bool TryLoadDataForCurrentUser(out EditorStartupDefinition data)
        {
            data = null;
            string[] guids = AssetDatabase.FindAssets("t:EditorStartupDefinition");
            var foundDatas = new List<EditorStartupDefinition>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<EditorStartupDefinition>(path);
                if (asset != null)
                    foundDatas.Add(asset);
            }

            string currentUser = GetCurrentWindowsUser();

            foreach (EditorStartupDefinition esd in foundDatas)
            {
                foreach (string developName in esd.Developers)
                {
                    if (developName.ToLower() == currentUser.ToLower())
                    {
                        data = esd;
                        Debug.Log($"Загружен актив EditorStartupDefinition для разработчика '{currentUser}'");
                        return true;
                    }
                }
            }
#if LOG
            Debug.LogWarning($"Пользователь {currentUser} не зарегистрирован в EditorStartupDefinition");
#endif
            return false;
        }

        public static Texture2D FindIconByName(string fileNameWithoutExtension)
        {
            // ищем Texture2D с таким именем по всему проекту
            string filter = $"{fileNameWithoutExtension} t:Texture2D";
            string[] guids = AssetDatabase.FindAssets(filter); // ищет по имени файла без расширения[web:315][web:317][web:318][web:320]

            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }


        /// <summary>
        /// Вспомогательный метод для получения текущего пользователя редактора (НЕ УСПЕВАЕТ ЗАГРУЗИТЬСЯ)
        /// </summary>
        public static string GetCurrentEditorUser() { return CloudProjectSettings.userName; }

        /// <summary>
        // Имя пользователя Windows
        /// </summary>
        public static string GetCurrentWindowsUser() { return Environment.UserName; }

        /// <summary>
        // Имя компьютера
        /// </summary>
        public static string GetCurrentComputerName() { return Environment.MachineName; }

        /// <summary>
        // Имя сетевого домена, связанное с текущим пользователем
        /// </summary>
        public static string GetCurrentUserDomainName() { return Environment.UserDomainName; }

        /// <summary>
        // Имя проекта как fallback
        /// </summary>
        public static string GetCurrentProjectName() { return Path.GetFileNameWithoutExtension(Application.dataPath); }
    }

    /// <summary>
    /// Пример окна на UI Toolkit, чтобы показать базовую интеграцию.
    /// </summary>
    public class EditorStartupWindow : EditorWindow
    {
        /// <summary>
        /// Метод вызывается движком при создании/открытии окна.
        /// </summary>
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            var titleLabel = new Label("Инициализация редактора выполнена");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 13;
            titleLabel.style.marginBottom = 4;

            var descriptionLabel = new Label(
                "Этот инструмент запущен автоматически после старта редактора.\n" +
                "Здесь можно вызывать любые нужные методы и отображать статус.");
            descriptionLabel.style.marginBottom = 8;

            var rerunButton = new Button(EditorStartupRunner.Run);
            rerunButton.text = "Повторно выполнить Run()";

            root.Add(titleLabel);
            root.Add(descriptionLabel);
            root.Add(rerunButton);
        }
    }
}

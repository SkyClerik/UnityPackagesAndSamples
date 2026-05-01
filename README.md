# EditorExtensions

Набор editor-инструментов для Unity: сборка объектов, грид-раскладка, замена мешей и другие утилиты.
Смотри меню > SkyClerik > EditorHub

## Contents
  
	SelectionTabContextMenu
		SelectionTabContextMenu добавляет пункт Assets → Selection Tab Window в контекстное меню Project. При вызове он открывает окно SelectionTabWindow, которое считывает текущее выделение в Project и показывает доступные сервисы в виде кнопок. Каждый сервис сам решает, доступен ли он для этого выделения, выполняет свою логику и при необходимости закрывает таб.

		TabServices
			FixUxmlDocumentPathsService
				(В разработке) Пытается починить сломанные пути в uxml документах
			ExportSelectedPrefabsService
				Экспортирует выделенные префабы в .unitypackage вместе с зависимостями
			ExampleSelectionTabService
				Пример для разработки сервисов
			ApplyMeshToNewFbxService
				Применение текущего размера как единица через пересоздание объекта в проекте.
	ComponentMeshReplacer
		Компонент для подмены меша
	ComponentStateHandler
		Компонент для сохранения и загрузки значений полей компонента. Кастомный шаблон настроек включая приватные.
	CustomHierarchy
		Позволяет настроить визуальное отображение элементов окна Hierarchy
	DocumentDataExtractor
		Собирает структуру  UI документа в лог консоли
	EditorHub
		Общее окно с зарегистрированными в редакторе расширениями
	EditorStartupRunner
		Сервис при старте редактора собирает в хаб расширения и может отображать окно при старте.
	FindMissingComponents
		Ищет на сцене компоненты с потерянными ссылками
	ObjectsCollectionToolkit
		Дополнительный overlay для окна Scene. Имеет кнопки для работы с выделенными объектами.
	ProjectStructureCollector
		Собирает структуру проекта в один текст

# Extensions

## Contents

	Components
		UserInterfaceRaycaster : MonoBehaviour
			Проверяет входжение в элементы UI Toolkit
		Singleton : MonoBehaviour
			Реализация паттерна
			
	DateTimeWrapper
	ReadOnlyAttribute
	ColorExt
	ComponentExt
	DictionaryExt
	EnumExt
	GameObjectExt
	GUILayoutExt
	ImageExt
	IntExt
	LayerMaskExt
	ListExt
	ProgressBarExt
	SoundExt
	StringExt
	ToolboxExt
	ToolkitExt
	TransformExt
	VectorExt
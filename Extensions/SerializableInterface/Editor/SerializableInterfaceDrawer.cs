using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SkyClerik
{
    [CustomPropertyDrawer(typeof(SerializableInterface), true)]
    public class SerializableInterfaceDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Корневой контейнер для поля
            var root = new VisualElement();

            // Находим SerializedProperty с Unity-ссылкой, как и раньше
            SerializedProperty gameObjectProperty = property.FindPropertyRelative("unityObjectReference");

            // Тип интерфейса по умолчанию
            Type interfaceType = typeof(ISerializableInterface);

            // Пытаемся вычислить конкретный T из SerializableInterface<T>
            Object containingObject = property.serializedObject.targetObject;
            Type containingObjectType = containingObject.GetType();
            FieldInfo field = GetNestedField(containingObjectType, property.propertyPath);

            if (field != null)
            {
                Type serializableInterfaceType = FindAncestorSerializableType(field.FieldType);
                if (serializableInterfaceType != null && serializableInterfaceType.IsGenericType)
                {
                    interfaceType = serializableInterfaceType.GetGenericArguments()[0];
                }
            }

            // Создаем ObjectField с фильтрацией по интерфейсу
            var objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(Object),            // базовый UnityEngine.Object
                allowSceneObjects = true,               // как в обычном ObjectField
                bindingPath = gameObjectProperty.propertyPath
            };

            // Дополнительно можно повесить проверку, чтобы не пускать не тот тип
            objectField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == null)
                {
                    gameObjectProperty.objectReferenceValue = null;
                }
                else
                {
                    if (!interfaceType.IsInstanceOfType(evt.newValue))
                    {
                        // Если не реализует нужный интерфейс — откатываем
                        Debug.LogWarning($"Assigned object does not implement required interface {interfaceType.Name}");
                        objectField.SetValueWithoutNotify(evt.previousValue);
                        return;
                    }

                    gameObjectProperty.objectReferenceValue = evt.newValue;
                }

                property.serializedObject.ApplyModifiedProperties();
            });

            // Привязываем к SerializedObject, чтобы работал биндинг
            root.Add(objectField);
            root.Bind(property.serializedObject);

            return root;
        }

        static FieldInfo GetNestedField(Type owningType, string fieldPath)
        {
            while (true)
            {
                int firstDotIndex = fieldPath.IndexOf(".", StringComparison.Ordinal);
                if (firstDotIndex > 0)
                {
                    string parentFieldName = fieldPath.Substring(0, firstDotIndex);
                    FieldInfo parentField = owningType.GetField(parentFieldName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (parentField == null)
                        return null;

                    owningType = parentField.FieldType;
                    fieldPath = fieldPath.Substring(firstDotIndex + 1);
                }
                else
                {
                    return owningType.GetField(fieldPath,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
        }

        static Type FindAncestorSerializableType(Type childType)
        {
            while (childType != null &&
                   (!childType.IsGenericType || childType.GetGenericTypeDefinition() != typeof(SerializableInterface<>)))
            {
                childType = childType.BaseType;
            }

            return childType;
        }
    }
}
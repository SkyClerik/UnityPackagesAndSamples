using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    public class EditorComponentStateHandler : MonoBehaviour
    {
        [Tooltip("включать/выключать рефлексию")]
        public bool useReflectionForHidden;

        public List<Component> components = new List<Component>();
        public List<ComponentsStateSO> stateAssets = new List<ComponentsStateSO>();

        public void SaveAllTo(ComponentsStateSO asset)
        {
#if UNITY_EDITOR
            if (asset == null)
            {
                Debug.LogWarning("SaveAllTo: asset == null");
                return;
            }

            asset.fields.Clear();

            foreach (var e in components)
            {
                if (e == null)
                    continue;

                SaveComponent(e, asset);
            }

            UnityEditor.EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

        public void LoadAllFrom(ComponentsStateSO asset)
        {
#if UNITY_EDITOR
            if (asset == null)
            {
                Debug.LogWarning("LoadAllFrom: asset == null");
                return;
            }

            foreach (var e in components)
            {
                if (e == null)
                    continue;

                LoadComponent(e, asset);
            }
#endif
        }

#if UNITY_EDITOR
        string GetComponentId(Component c)
        {
            var type = c.GetType();
            var siblings = c.GetComponents(type);
            int localIndex = System.Array.IndexOf(siblings, c);
            return $"{c.gameObject.name}:{type.Name}:{localIndex}";
        }

        void SaveComponent(Component comp, ComponentsStateSO targetAsset)
        {
            string compId = GetComponentId(comp);

            var so = new UnityEditor.SerializedObject(comp);
            var prop = so.GetIterator();

            if (!prop.NextVisible(true))
                return;

            while (prop.NextVisible(false))
            {
                if (prop.name == "m_Script")
                    continue;

                var entry = new ComponentsStateSO.FieldEntry
                {
                    componentId = compId,
                    fieldPath = prop.propertyPath
                };

                if (!FillFieldEntryFromProperty(entry, prop))
                    continue;

                targetAsset.fields.Add(entry);
            }

            // ДОПОЛНИТЕЛЬНО: рефлексия
            if (useReflectionForHidden)
                SaveHiddenByReflection(compId, comp, targetAsset);
        }

        void LoadComponent(Component comp, ComponentsStateSO sourceAsset)
        {
            string compId = GetComponentId(comp);

            var so = new UnityEditor.SerializedObject(comp);

            foreach (var fe in sourceAsset.fields)
            {
                if (fe.componentId != compId)
                    continue;

                var prop = so.FindProperty(fe.fieldPath);
                if (prop == null)
                    continue;

                ApplyFieldEntryToProperty(fe, prop);
            }

            so.ApplyModifiedProperties();
            UnityEditor.EditorUtility.SetDirty(comp);

            // ДОПОЛНИТЕЛЬНО: рефлексия
            if (useReflectionForHidden)
                LoadHiddenByReflection(compId, comp, sourceAsset);
        }

        bool FillFieldEntryFromProperty(ComponentsStateSO.FieldEntry entry, UnityEditor.SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Integer:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Int;
                    entry.intValue = prop.intValue;
                    return true;

                case UnityEditor.SerializedPropertyType.Float:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Float;
                    entry.floatValue = prop.floatValue;
                    return true;

                case UnityEditor.SerializedPropertyType.Boolean:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Bool;
                    entry.boolValue = prop.boolValue;
                    return true;

                case UnityEditor.SerializedPropertyType.String:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.String;
                    entry.stringValue = prop.stringValue;
                    return true;

                case UnityEditor.SerializedPropertyType.Vector2:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Vector2;
                    entry.vector2Value = prop.vector2Value;
                    return true;

                case UnityEditor.SerializedPropertyType.Vector3:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Vector3;
                    entry.vector3Value = prop.vector3Value;
                    return true;

                case UnityEditor.SerializedPropertyType.Color:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Color;
                    entry.colorValue = prop.colorValue;
                    return true;

                case UnityEditor.SerializedPropertyType.ObjectReference:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Object;
                    entry.objectValue = prop.objectReferenceValue;
                    return true;

                case UnityEditor.SerializedPropertyType.Enum:
                    entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Enum;
                    entry.enumIndex = prop.enumValueIndex;
                    return true;

                default:
                    return false;
            }
        }

        void ApplyFieldEntryToProperty(ComponentsStateSO.FieldEntry entry, UnityEditor.SerializedProperty prop)
        {
            switch (entry.valueType)
            {
                case ComponentsStateSO.FieldEntry.ValueType.Int:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Integer)
                        prop.intValue = entry.intValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Float:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Float)
                        prop.floatValue = entry.floatValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Bool:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Boolean)
                        prop.boolValue = entry.boolValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.String:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.String)
                        prop.stringValue = entry.stringValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Vector2:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Vector2)
                        prop.vector2Value = entry.vector2Value;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Vector3:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Vector3)
                        prop.vector3Value = entry.vector3Value;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Color:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Color)
                        prop.colorValue = entry.colorValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Object:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                        prop.objectReferenceValue = entry.objectValue;
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Enum:
                    if (prop.propertyType == UnityEditor.SerializedPropertyType.Enum)
                        prop.enumValueIndex = entry.enumIndex;
                    break;
            }
        }

        void SaveHiddenByReflection(string compId, Component comp, ComponentsStateSO asset)
        {
            var type = comp.GetType();

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public;

            // ПОЛЯ
            var fields = type.GetFields(flags);
            foreach (var f in fields)
            {
                // мы НЕ хотим дублировать то, что уже сериализует Unity:
                // Unity обычно сериализует public или [SerializeField] поля.
                // Здесь можно фильтровать только чистые private без SerializeField,
                // но у Unity-внутренностей атрибуты могут быть свои, поэтому делаем
                // максимально простой фильтр: пропускаем "m_Script" и статические.
                if (f.IsStatic)
                    continue;
                if (f.Name == "m_Script")
                    continue;

                // типы — те же, что у нас в FillFieldEntryFromProperty
                var value = f.GetValue(comp);
                if (value == null)
                    continue;

                var entry = new ComponentsStateSO.FieldEntry
                {
                    componentId = compId,
                    fieldPath = "ref:field:" + f.Name
                };

                if (!FillFieldEntryFromObject(entry, value, f.FieldType))
                    continue;

                asset.fields.Add(entry);
            }

            // СВОЙСТВА (например, Type у Light)
            var props = type.GetProperties(flags);
            foreach (var p in props)
            {
                if (!p.CanRead || !p.CanWrite)
                    continue;
                if (p.GetIndexParameters().Length > 0)
                    continue; // индексаторы не трогаем

                object value;
                try
                {
                    value = p.GetValue(comp, null);
                }
                catch
                {
                    continue; // некоторые Unity свойства могут бросать
                }
                if (value == null)
                    continue;

                var entry = new ComponentsStateSO.FieldEntry
                {
                    componentId = compId,
                    fieldPath = "ref:prop:" + p.Name
                };

                if (!FillFieldEntryFromObject(entry, value, p.PropertyType))
                    continue;

                asset.fields.Add(entry);
            }
        }

        void LoadHiddenByReflection(string compId, Component comp, ComponentsStateSO asset)
        {
            var type = comp.GetType();

            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public;

            foreach (var fe in asset.fields)
            {
                if (fe.componentId != compId)
                    continue;
                if (!fe.fieldPath.StartsWith("ref:"))
                    continue;

                // ref:field:Name или ref:prop:Name
                var parts = fe.fieldPath.Split(':');
                if (parts.Length != 3)
                    continue;

                var kind = parts[1];
                var name = parts[2];

                if (kind == "field")
                {
                    var field = type.GetField(name, flags);
                    if (field == null)
                        continue;

                    object value;
                    if (!GetObjectFromFieldEntry(fe, field.FieldType, out value))
                        continue;

                    field.SetValue(comp, value);
                }
                else if (kind == "prop")
                {
                    var prop = type.GetProperty(name, flags);
                    if (prop == null || !prop.CanWrite)
                        continue;

                    object value;
                    if (!GetObjectFromFieldEntry(fe, prop.PropertyType, out value))
                        continue;

                    try
                    {
                        prop.SetValue(comp, value, null);
                    }
                    catch
                    {
                        // игнорируем, если Unity не даёт сетнуть
                    }
                }
            }
        }


        bool FillFieldEntryFromObject(ComponentsStateSO.FieldEntry entry, object value, System.Type type)
        {
            if (type == typeof(int))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Int;
                entry.intValue = (int)value;
                return true;
            }
            if (type == typeof(float))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Float;
                entry.floatValue = (float)value;
                return true;
            }
            if (type == typeof(bool))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Bool;
                entry.boolValue = (bool)value;
                return true;
            }
            if (type == typeof(string))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.String;
                entry.stringValue = (string)value;
                return true;
            }
            if (type == typeof(Vector2))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Vector2;
                entry.vector2Value = (Vector2)value;
                return true;
            }
            if (type == typeof(Vector3))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Vector3;
                entry.vector3Value = (Vector3)value;
                return true;
            }
            if (type == typeof(Color))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Color;
                entry.colorValue = (Color)value;
                return true;
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Object;
                entry.objectValue = (UnityEngine.Object)value;
                return true;
            }
            if (type.IsEnum)
            {
                entry.valueType = ComponentsStateSO.FieldEntry.ValueType.Enum;
                entry.enumIndex = (int)value;
                return true;
            }

            return false;
        }

        bool GetObjectFromFieldEntry(ComponentsStateSO.FieldEntry entry, System.Type type, out object value)
        {
            value = null;

            switch (entry.valueType)
            {
                case ComponentsStateSO.FieldEntry.ValueType.Int:
                    if (type == typeof(int) || type.IsEnum)
                    {
                        value = type.IsEnum ? System.Enum.ToObject(type, entry.intValue) : (object)entry.intValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Float:
                    if (type == typeof(float))
                    {
                        value = entry.floatValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Bool:
                    if (type == typeof(bool))
                    {
                        value = entry.boolValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.String:
                    if (type == typeof(string))
                    {
                        value = entry.stringValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Vector2:
                    if (type == typeof(Vector2))
                    {
                        value = entry.vector2Value;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Vector3:
                    if (type == typeof(Vector3))
                    {
                        value = entry.vector3Value;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Color:
                    if (type == typeof(Color))
                    {
                        value = entry.colorValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Object:
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    {
                        value = entry.objectValue;
                        return true;
                    }
                    break;

                case ComponentsStateSO.FieldEntry.ValueType.Enum:
                    if (type.IsEnum)
                    {
                        value = System.Enum.ToObject(type, entry.enumIndex);
                        return true;
                    }
                    break;
            }

            return false;
        }

#endif
    }
}
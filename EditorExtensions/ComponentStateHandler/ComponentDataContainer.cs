using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    [CreateAssetMenu(fileName = "ComponentsState", menuName = "SkyClerik/Editor/ComponentStates")]
    public class ComponentsStateSO : ScriptableObject
    {
        [Serializable]
        public class FieldEntry
        {
            public string fieldPath;
            public string componentId;

            public enum ValueType : byte
            {
                Int,
                Float,
                Bool,
                String,
                Vector2,
                Vector3,
                Color,
                Object,
                Enum
            }

            public ValueType valueType;

            public int intValue;
            public float floatValue;
            public bool boolValue;
            public string stringValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;
            public Color colorValue;
            public UnityEngine.Object objectValue;
            public int enumIndex;
        }

        public List<FieldEntry> fields = new List<FieldEntry>();
    }
}
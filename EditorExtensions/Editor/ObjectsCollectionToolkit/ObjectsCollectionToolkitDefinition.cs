using UnityEngine;

namespace SkyClerik
{
    [CreateAssetMenu(fileName = "new ObjectsCollectionToolkitDefinition", menuName = "SkyClerik/Editor/ObjectsCollectionToolkitDefinition")]
    public class ObjectsCollectionToolkitDefinition : ScriptableObject
    {
        [Header("Icons")]

        [SerializeField]
        public string _applicationDescription = "Тут будет описание приложения.";
        [SerializeField]
        public Texture2D _iconCollection;
        [SerializeField]
        public Texture2D _iconSelectRoot;
        [SerializeField]
        public Texture2D _iconMerge;
        [SerializeField]
        public Texture2D _iconClearParent;
        [SerializeField]
        public Texture2D _iconRotate;
        [SerializeField]
        private Texture2D _iconRoundItUp;
        [SerializeField]
        private Texture2D _iconGreed;
        [SerializeField]
        private float _rotateAnge = 90f;

        [Header("Settings")]

        [SerializeField]
        private ERoundStep _roundStep = ERoundStep.Quarter;
        public enum ERoundStep
        {
            [InspectorName("One (1.0)")]
            One = 1,
            [InspectorName("Half (0.5)")]
            Half = 2,
            [InspectorName("Quarter (0.25)")]
            Quarter = 4,
            [InspectorName("Eighth (0.125)")]
            Eighth = 8,
            [InspectorName("Sixteenth (0.0625)")]
            Sixteenth = 16
        }

        public string ApplicationDescription => _applicationDescription;
        public Texture2D IconCollection => _iconCollection;
        public Texture2D IconSelectRoot => _iconSelectRoot;
        public Texture2D IconMerge => _iconMerge;
        public Texture2D IconClearParent => _iconClearParent;
        public Texture2D IconRotate => _iconRotate;
        public Texture2D IconGreed => _iconGreed;
        public Texture2D IconRoundItUp => _iconRoundItUp;
        public float RotateAnge => _rotateAnge;
        public ERoundStep RoundStep => _roundStep;


        [Header("Grid")]

        [SerializeField]
        [Tooltip("Размер шага сетки по XZ")]
        private Vector2Int _gridOffset = new Vector2Int(2, 2);
        [SerializeField]
        [Tooltip("Сколько объектов в строке")]
        private int _gridStack = 5;

        public Vector2 GridOffset => _gridOffset;
        public int GridStack => _gridStack;

    }
}
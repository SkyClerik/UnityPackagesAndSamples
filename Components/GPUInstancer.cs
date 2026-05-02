using UnityEngine;
using System.Collections.Generic;

namespace SkyClerik
{
    public class GPUInstancer : MonoBehaviour
    {
        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        [Tooltip("Включи, если объекты в списке будут двигаться (дроны)")]
        [SerializeField] private bool _isDynamic;

        [SerializeField] private List<GameObject> _targets = new List<GameObject>();

        private Matrix4x4[] _matrices;
        private int _activeCount;

        private void Start()
        {
            foreach (var go in _targets)
            {
                if (go.TryGetComponent<MeshRenderer>(out var mr))
                    mr.enabled = false;
            }
            RecalculateMatrices();
        }

        // Вызывать вручную, если кто-то включился/выключился (SetActive)
        public void RecalculateMatrices()
        {
            var tempMatrices = new List<Matrix4x4>();
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i] != null && _targets[i].activeInHierarchy)
                {
                    tempMatrices.Add(_targets[i].transform.localToWorldMatrix);
                }
            }
            _matrices = tempMatrices.ToArray();
            _activeCount = _matrices.Length;
        }

        private void Update()
        {
            if (_activeCount == 0) return;

            // Если динамика включена — обновляем координаты в массиве каждый кадр
            if (_isDynamic)
            {
                int index = 0;
                for (int i = 0; i < _targets.Count; i++)
                {
                    if (_targets[i] != null && _targets[i].activeInHierarchy)
                    {
                        _matrices[index] = _targets[i].transform.localToWorldMatrix;
                        index++;
                    }
                }
            }

            // Рисуем
            Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrices, _activeCount);
        }
    }
}

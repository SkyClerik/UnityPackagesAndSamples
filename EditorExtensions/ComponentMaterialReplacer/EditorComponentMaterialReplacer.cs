using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkyClerik
{
    public class EditorComponentMaterialReplacer : MonoBehaviour
    {
        [Header("Помогает заменить материалы на указанных объектах.")]
        [SerializeField]
        private Material _material;
        [SerializeField]
        private List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();

        public Material GetMaterial => _material;

        public void AddChildrenToList()
        {
            _meshRenderers = gameObject.transform.GetComponentsInChildren<MeshRenderer>(includeInactive: true).ToList();
        }

        public void ReplaceMaterial()
        {
            foreach (var mashRenderer in _meshRenderers)
            {
                if (mashRenderer == null)
                    continue;

                mashRenderer.material = _material;
            }
        }
    }
}
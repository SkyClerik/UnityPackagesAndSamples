using UnityEngine;

namespace SkyClerik
{
    public class GizmoArea : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _wireCubeSize = Vector3.one;

        [SerializeField]
        private Color _wireColor = Color.green;

        [SerializeField]
        private bool _centerAtTransformPosition = true;
        // true  – центр куба в transform.position
        // false – куб стоит на позиции объекта (transform.position — нижняя грань по Y)

        private void OnDrawGizmos()
        {
            if (_wireCubeSize == Vector3.zero)
                return;

            Gizmos.color = _wireColor;

            Vector3 center = transform.position;

            if (_centerAtTransformPosition)
            {
                // Режим 1: transform.position — центр куба
                // center остаётся как есть
            }
            else
            {
                // Режим 2: куб стоит на позиции объекта
                center.y += _wireCubeSize.y * 0.5f;
            }

            Gizmos.DrawWireCube(center, _wireCubeSize);
        }
    }
}
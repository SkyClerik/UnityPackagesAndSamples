using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helper
{
    public class EditorComponentObjectsGridPlacer : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> _objects = new List<Transform>();

        [SerializeField]
        private Vector2 _offset = new Vector2(0, 1);
        [SerializeField]
        private int _stack = 5;

        public void AddChildrenToList()
        {
            //_objects = gameObject.transform.GetComponentsInChildren<Transform>(includeInactive: true).ToList();

            _objects = GetComponentsInChildren<Transform>(includeInactive: true).Where(t => t != transform).ToList();
        }

        public void SetPlacer()
        {
            Vector2 offset = _offset;
            int stack = 0;
            foreach (Transform obj in _objects)
            {
                //obj.transform.position = new Vector3(offset.x, 0, offset.y);
                obj.localPosition = new Vector3(offset.x, 0, offset.y);
                stack++;

                offset.x += _offset.x;

                if (stack >= _stack)
                {
                    offset.x = _offset.x;
                    offset.y += _offset.y;
                    stack = 0;
                }
            }
        }
    }
}
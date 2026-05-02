using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    public abstract class TypedEntityContainer<T> : MonoBehaviour where T : Component
    {
        protected readonly Dictionary<System.Type, List<T>> _entitiesByType = new();

        public void Register(T entity)
        {
            if (entity == null) return;
            var type = entity.GetType();

            if (!_entitiesByType.TryGetValue(type, out var list))
            {
                list = new List<T>();
                _entitiesByType[type] = list;
            }

            if (!list.Contains(entity))
                list.Add(entity);
        }

        public void Unregister(T entity)
        {
            if (entity == null) return;
            var type = entity.GetType();

            if (_entitiesByType.TryGetValue(type, out var list))
                list.Remove(entity);
        }

        public bool TryGetNearest(Vector3 from, System.Type type, out T nearest)
        {
            nearest = null;
            if (!_entitiesByType.TryGetValue(type, out var list) || list.Count == 0)
                return false;

            float bestSqr = float.MaxValue;
            bool found = false;

            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (e == null) continue;

                float sqr = (e.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = e;
                    found = true;
                }
            }
            return found;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SkyClerik
{
    public abstract class EntityContainer<T> : MonoBehaviour where T : Component
    {
        protected readonly List<T> _entities = new List<T>();

        public void Register(T entity)
        {
            if (entity != null && !_entities.Contains(entity))
                _entities.Add(entity);
        }

        public void Unregister(T entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>
        /// Возвращает список всех зарегистрированных объектов.
        /// </summary>
        public IReadOnlyList<T> GetAll() => _entities;

        /// <summary>
        /// Находит ближайший объект. Возвращает true, если объект найден.
        /// </summary>
        public bool TryGetNearest(Vector3 from, out T nearest)
        {
            nearest = null;
            float bestSqr = float.MaxValue;
            bool found = false;

            for (int i = 0; i < _entities.Count; i++)
            {
                var e = _entities[i];
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

        /// <summary>
        /// Простая проверка на наличие объектов в контейнере.
        /// </summary>
        public bool HasAny() => _entities.Count > 0;

        /// <summary>
        /// Очистка контейнера (например, при смене уровня).
        /// </summary>
        public void ClearAll() => _entities.Clear();
    }
}

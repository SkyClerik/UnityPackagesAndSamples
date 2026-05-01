using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Toolbox
{
    public static partial class ToolboxExt
    {
        public interface IChance
        {
            /// <summary>
            /// Вес или шанс выбора этого объекта.
            /// </summary>
            float returnChance { get; set; }
        }

        /// <summary>
        /// Возвращает случайный элемент из списка.
        /// </summary>
        /// <typeparam name="T">Тип элементов в списке.</typeparam>
        /// <param name="list">Список, из которого нужно выбрать случайный элемент.</param>
        /// <returns>Случайно выбранный элемент типа T из списка.</returns>
        public static T Random<T>(this List<T> list)
        {
            var val = list[UnityEngine.Random.Range(0, list.Count)];
            return val;
        }

        /// <summary>
        /// Используя рефлексию, находит в объекте числовое поле (int или float) по имени и **увеличивает** его значение. Поиск имени поля нечувствителен к регистру.
        /// </summary>
        /// <remarks>
        /// **Осторожно:** этот метод использует небезопасные и медленные операции с рефлексией. Он может изменять приватные поля. Используйте с большой осторожностью. Работает только с полями типа `int` и `float`.
        /// </remarks>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="_type">Объект, поле которого нужно изменить.</param>
        /// <param name="name">Имя поля для изменения (без учета регистра).</param>
        /// <param name="value">Значение, на которое нужно увеличить поле.</param>
        /// <returns>Возвращает измененный объект `_type`.</returns>
        public static T IncrementFieldByName<T>(T _type, string name, float value)
        {
            FieldInfo[] myField = _type.GetType().GetFields();

            for (int i = 0; i < myField.Length; i++)
            {
                if (myField[i].Name.ToLower() == name.ToLower())
                {
                    switch (myField[i].GetValue(_type))
                    {
                        case int a:
                            myField[i].SetValueDirect(__makeref(_type), a + value);
                            break;
                        case float a:
                            myField[i].SetValueDirect(__makeref(_type), a + value);
                            break;
                    }
                }
            }
            return _type;
        }

        /// <summary>
        /// Выбирает случайный элемент из списка на основе весов (шансов), определенных в свойстве `returnChance` каждого элемента.
        /// </summary>
        /// <typeparam name="T">Тип элементов в списке. Должен реализовывать интерфейс `IChance`.</typeparam>
        /// <param name="obj">Список объектов для взвешенного случайного выбора.</param>
        /// <returns>Случайно выбранный элемент из списка с учетом его шанса. Если все шансы равны нулю, может вернуть первый элемент.</returns>
        public static T RandomByChance<T>(this List<T> obj) where T : IChance
        {
            var total = 0f;
            var probs = new float[obj.Count];

            for (int i = 0; i < probs.Length; i++)
            {
                probs[i] = obj[i].returnChance;
                total += probs[i];
            }
            System.Random _r = new System.Random();
            var randomPoint = (float)_r.NextDouble() * total;

            for (int i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                    return obj[i];
                randomPoint -= probs[i];
            }
            return obj[0];
        }

        /// <summary>
        /// Находит **последний** индекс `null` элемента в массиве.
        /// </summary>
        /// <remarks>
        /// **Внимание:** В текущей реализации метод всегда возвращает `false`, даже если `null` элемент найден. 
        /// Вероятно, предполагалось, что он должен возвращать `true` и выходить из цикла при первом же нахождении `null`.
        /// </remarks>
        /// <typeparam name="T">Тип элементов в массиве.</typeparam>
        /// <param name="array">Массив для поиска.</param>
        /// <param name="num">Выходной параметр, содержащий индекс последнего найденного `null` элемента, или 0, если `null` элементы не найдены.</param>
        /// <returns>Всегда возвращает `false` в текущей реализации.</returns>
        public static bool FindLastNullIndex<T>(this T[] array, out int num)
        {
            num = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                    num = i;
            }
            return false;
        }

        /// <summary>
        /// Генерирует и возвращает новый глобально уникальный идентификатор (GUID) в виде строки.
        /// </summary>
        /// <returns>Строковое представление нового GUID.</returns>
        public static string GetNewID()
        {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }

        /// <summary>
        /// Вычисляет 2D-расстояние между двумя Transform'ами на плоскости XZ, игнорируя координату Y.
        /// </summary>
        /// <param name="ATransform">Первый Transform.</param>
        /// <param name="BTransform">Второй Transform.</param>
        /// <returns>Расстояние между объектами на плоскости XZ.</returns>
        public static float GetDistance(Transform ATransform, Transform BTransform)
        {
            Vector3 AVector = ATransform.position;
            Vector3 BVector = BTransform.position;

            return Vector3.Distance(new Vector2(AVector.x, AVector.z), new Vector2(BVector.x, BVector.z));
        }

        /// <summary>
        /// Проверяет, является ли *хотя бы один* элемент в массиве `null`.
        /// </summary>
        /// <remarks>
        /// Если массив `array` сам является `null`, метод возвращает `true`.
        /// </remarks>
        /// <typeparam name="T">Тип элементов в массиве.</typeparam>
        /// <param name="array">Массив для проверки.</param>
        /// <returns>Возвращает `true`, если массив `null` или содержит хотя бы один `null` элемент; иначе `false`.</returns>
        public static bool IsNullAny<T>(params T[] array)
        {
            if (array == null)
                return true;

            return array.Any(value => value == null);
        }

        /// <summary>
        /// Меняет местами значения двух переменных с помощью временной переменной.
        /// </summary>
        /// <typeparam name="T">Тип переменных.</typeparam>
        /// <param name="lhs">Первая переменная (левая сторона).</param>
        /// <param name="rhs">Вторая переменная (правая сторона).</param>
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        /// <summary>
        /// Creates and returns a clone of any given scriptable object.
        /// </summary>
        public static T Clone<T>(this T scriptableObject) where T : ScriptableObject
        {
            if (scriptableObject == null)
            {
                Debug.LogError($"ScriptableObject was null. Returning default {typeof(T)} object.");
                return (T)ScriptableObject.CreateInstance(typeof(T));
            }

            T instance = Object.Instantiate(scriptableObject);
            instance.name = scriptableObject.name; // remove (Clone) from name
            return instance;
        }
    }
}

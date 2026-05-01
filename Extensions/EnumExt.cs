using System;
using System.Linq;

namespace UnityEngine.Toolbox
{
    public static partial class EnumExt
    {
        /// <summary>
        /// Возвращает случайное значение из указанного перечисления (enum).
        /// </summary>
        /// <typeparam name="T">Тип перечисления (enum), из которого нужно получить случайное значение.</typeparam>
        /// <returns>Случайное значение типа T.</returns>
        public static T GetRandom<T>() where T : System.Enum
        {
            System.Random random = new System.Random();
            System.Array array = System.Enum.GetValues(typeof(T));
            return (T)array.GetValue(random.Next(array.Length));
        }

        /// <summary>
        /// Проверяет, содержится ли текущий элемент в переданном наборе значений.
        /// </summary>
        /// <example>
        /// <code>
        /// DayOfWeek today = DayOfWeek.Monday;
        /// bool isWeekend = today.In(DayOfWeek.Saturday, DayOfWeek.Sunday); // false
        /// </code>
        /// </example>
        /// <typeparam name="T">Тип значения (обычно enum).</typeparam>
        /// <param name="value">Значение, наличие которого нужно проверить.</param>
        /// <param name="array">Набор значений, в котором производится поиск.</param>
        /// <returns>Возвращает `true`, если `value` найдено в `array`, иначе `false`.</returns>
        public static bool In<T>(this T value, params T[] array)
        {
            return array.Contains(value);
        }

        /// <summary>
        /// Преобразует перечисляемое значение, у которого внутренний тип — byte, в byte.
        /// Используется только если базовый тип enum — byte (например, enum : byte).
        /// </summary>
        /// <typeparam name="T">Тип перечисления, обязательно : struct, Enum</typeparam>
        /// <param name="value">Значение перечисления</param>
        /// <returns>Значение перечисления как байт</returns>
        public static byte ToByte<T>(this T value) where T : struct, Enum
        {
            return (byte)(object)value;
        }

        /// <summary>
        /// Преобразует значение перечисления в его базовое целочисленное представление (int).
        /// Подходит для любых enum-типов, независимо от базового типа (byte, int и т.п.).
        /// </summary>
        /// <typeparam name="T">Тип перечисления, обязательно : struct, Enum</typeparam>
        /// <param name="value">Значение перечисления</param>
        /// <returns>Целочисленное значение enum</returns>
        public static int ToInt<T>(this T value) where T : struct, Enum
        {
            return (int)(object)value;
        }
    }
}
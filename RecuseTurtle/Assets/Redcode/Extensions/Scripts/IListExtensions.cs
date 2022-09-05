using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Redcode.Extensions
{
    public static class IListExtensions
    {
        /// <summary>
        /// Pops element by <paramref name="index"/>.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <param name="index">Index of element to pop.</param>
        /// <returns>The popped element.</returns>
        public static T Pop<T>(this IList<T> list, int index)
        {
            var element = list[index];
            list.RemoveAt(index);

            return element;
        }

        /// <summary>
        /// Pops elements by <paramref name="indexes"/>.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <param name="indexes">Indexes of elements to be popped.</param>
        /// <returns>The popped element.</returns>
        public static List<T> Pop<T>(this IList<T> list, params int[] indexes)
        {
            var popped = new List<T>();

            foreach (var index in indexes)
                popped.Add(list.Pop(index));

            return popped;
        }

        /// <summary>
        /// Pops random element from <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <returns>Popped element.</returns>
        public static T PopRandom<T>(this IList<T> list) => list.Pop(Random.Range(0, list.Count));

        /// <summary>
        /// Pops random elements from list.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <param name="count">Count of elements to be popped.</param>
        /// <returns>Popped elements.</returns>
        public static List<T> PopRandoms<T>(this IList<T> list, int count)
        {
            var popped = new List<T>();

            for (int i = 0; i < count; i++)
                popped.Add(list.PopRandom());

            return popped;
        }

        /// <summary>
        /// Pops random elements from list with specified probability.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <param name="probabilities"></param>
        /// <returns>Popped element.</returns>
        public static T PopRandomElementWithProbability<T>(this IList<T> list, params float[] probabilities)
        {
            return Pop(list, list.GetRandomElementIndexWithProbability(probabilities));
        }

        /// <summary>
        /// Pops random elements from list with specified probability.
        /// </summary>
        /// <typeparam name="T">Source type.</typeparam>
        /// <param name="list">List with elements.</param>
        /// <param name="probabilities"></param>
        /// <returns>Popped elements.</returns>
        public static T PopRandomElementWithProbability<T>(this IList<T> list, IEnumerable<float> probabilities)
        {
            return Pop(list, list.GetRandomElementIndexWithProbability(probabilities));
        }

        /// <summary>
        /// Removes all elements starts from <paramref name="index"/>.
        /// </summary>
        /// <typeparam name="T">Elements type.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="index">From what index need starts removing?</param>
        public static void RemoveRange<T>(this IList<T> list, int index)
        {
            for (int i = list.Count - 1; i >= index; i++)
                list.RemoveAt(i);
        }
    }
}
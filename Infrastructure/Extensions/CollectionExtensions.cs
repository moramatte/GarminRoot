using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Infrastructure.Extensions
{
    public static class CollectionExtensions
    {
		public static IDictionary<string, string[]> ToDictionary(this NameValueCollection @this)
		{
			var dict = new Dictionary<string, string[]>();

			if (@this != null)
			{
				foreach (string key in @this.AllKeys)
				{
                    if (key == null)
                        continue;
					dict.Add(key, @this.GetValues(key) ?? []);
				}
			}

			return dict;
		}

		public static bool TryFind<T>(this List<T> collection, Predicate<T> predicate, out T value)
        {
            value = default;
            var index = collection.FindIndex(predicate);
            if (index == -1)
                return false;
            value = collection[index];
            return true;
        }

        public static bool TryFind<T>(this IEnumerable<T> collection, Func<T, bool> predicate, out T value)
        {
            value = collection.FirstOrDefault(predicate);
            if (value == null)
                return false;
            return !value.Equals(default(T));
        }

        /// <summary>
        /// Returns a comma separated list of all items.ToString() in the collection.
        /// </summary>
        /// <param name="collection">Arbitrary IEnumerable</param>
        /// <returns>One string</returns>
        public static string ToCommaSeparatedString<T>(this IEnumerable<T> collection)
        {
            return string.Join(", ", collection);
        }

        public static string ToWhitespaceSeparatedString<T>(this IEnumerable<T> collection)
        {
            return string.Join(" ", collection);
        }

        /// <summary>
        /// Returns a linebreak separated list of all items.ToString() in the collection.
        /// </summary>
        /// <param name="collection">Arbitrary IEnumerable</param>
        /// <returns>One string</returns>
        public static string ToLinebreakSeparatedString<T>(this IEnumerable<T> collection)
        {
            return string.Join(Environment.NewLine, collection);
        }

        /// <summary>
        /// Returns a semicolon separated list of all items.ToString() in the collection.
        /// </summary>
        /// <param name="collection">Arbitrary IEnumerable</param>
        /// <returns>One string</returns>
        public static string ToSemicolonSeparatedString<T>(this IEnumerable<T> collection)
        {
            return string.Join(';', collection);
        }

        /// <summary>
        /// Negated Any()
        /// </summary>
        /// <param name="collection">Arbitrary IEnumerable</param>
        /// <param name="predicate">Qualifying boolean func</param>
        /// <returns>True if the collection is null or empty</returns>
        public static bool None<T>(this IEnumerable<T> collection, Func<T, bool> predicate = null)
        {
            if (collection == null)
            {
                return true;
            }

            if (predicate == null)
            {
                return !collection.Any();
            }

            return !collection.Any(predicate);
        }

        public static void AddOrReplace<T>(this IList theList, T item, Func<T, bool> replaceIf = null)
        {
            if (replaceIf == null)
            {
                if (theList.Contains(item))
                {
                    return; // Implicit update
                }
            }
            else
            {
                foreach (var i in theList)
                {
                    if (replaceIf((T)i))
                    {
                        theList.Remove(i);
                        break;
                    }
                }
            }

            theList.Add(item);
        }

        public static bool Lacks(this Dictionary<string, string> theLookupTable, string key)
        {
            return !theLookupTable.ContainsKey(key);
        }

        public static bool Lacks<T>(this IList theList, T item)
        {
            return !theList.Contains(item);
        }

        public static IEnumerable<T> Without<T>(this IEnumerable<T> theList, T item)
        {
            return theList.Except(new[] { item } );
        }

        public static IEnumerable<T> WashDuplicates<T>(this IEnumerable<T> itemsIn) where T : class
        {
            var result = new List<T>();
            var items = itemsIn.ToList();

            for (int i = 0; i < items.Count(); i++)
            {
                if (i > 0)
                {
                    if (items[i] == items[i - 1])
                    {
                        continue;
                    }
                }
                result.Add(items[i]);
            }

            return result;
        }

        public static void Remove<T>(this ConcurrentBag<T> bag, T item)
        {
            while (bag.Count > 0)
            {
                T result;
                bag.TryTake(out result);

                if (result.Equals(item))
                {
                    break;
                }

                bag.Add(result);
            }
        }

        private static Random _random = new();
		public static T TakeRandom<T>(this IEnumerable<T> elements)
		{
			if (elements == null || !elements.Any())
				return default;
			var array = elements.ToArray();
			return array.ElementAt(_random.Next(array.Length));
		}

		public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> enumerable) where T : class
		{
			return enumerable.Where(e => e != null).Select(e => e!);
		}
	}
}

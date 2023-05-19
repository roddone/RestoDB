namespace RestODb.Core
{
    public static class MiscExtensions
    {
        /// <summary>
        /// Determines whether the sequence is null or contains no elements.
        /// </summary>
        /// <typeparam name="T">The IEnumerable type.</typeparam>
        /// <param name="enumerable">The enumerable, which may be null or empty.</param>
        /// <returns><c>true</c> if the IEnumerable is null or empty; otherwise, <c>false</c>.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }

            /* If this is a list, use the Count property for efficiency. 
             * The Count property is O(1) while IEnumerable.Count() is O(N). */
            if (enumerable is ICollection<T> collection)
            {
                return collection.Count == 0;
            }

            return !enumerable.Any();
        }

        public static string[] SafeSplit(this string input, char separator = ',')
        {
            return input.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        }
    }
}

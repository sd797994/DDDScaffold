namespace Infrastructure.Object
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            int index = 0;
            foreach (var item in source)
            {
                action(index, item);
                index++;
            }
        }
    }
}

namespace SubtitlesApp.Core.Extensions;

public static class ListExtensions
{
    public static IEnumerable<List<T>> ChunkWithOverlap<T>(this List<T> source, int size, int overlap = 0)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than zero.", nameof(size));
        }

        if (overlap < 0)
        {
            throw new ArgumentException("Overlap must be non-negative.", nameof(overlap));
        }

        if (overlap >= size)
        {
            throw new ArgumentException("Overlap must be less than chunk size.", nameof(overlap));
        }

        for (int i = 0; i < source.Count; i += size - overlap)
        {
            yield return source.Skip(i).Take(size).ToList();
        }
    }
}

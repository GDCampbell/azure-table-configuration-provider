namespace AzureTable.Provider.Configuration.Mapping.Elements;

internal sealed class SectionPath
{
    public string Path { get; }

    /// <summary>
        /// Initializes a new SectionPath instance, normalizing a null or whitespace input to an empty path.
        /// </summary>
        /// <param name="path">The initial path value; if null or whitespace it is treated as an empty string.</param>
        private SectionPath(string path)
        => Path = string.IsNullOrWhiteSpace(path) ? string.Empty : path;

    public static SectionPath Root { get; } = new(string.Empty);

    /// <summary>
    /// Creates a new SectionPath by appending a single path segment to the current path using ':' as the separator.
    /// </summary>
    /// <param name="segment">The path segment to append; leading and trailing whitespace are ignored.</param>
    /// <returns>A new SectionPath representing the current path with <paramref name="segment"/> appended.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="segment"/> is null, empty, or consists only of whitespace.</exception>
    public SectionPath AddSegment(string segment)
    {
        segment = segment?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        return new(string.IsNullOrWhiteSpace(Path) ? segment : $"{Path}:{segment}");
    }

}
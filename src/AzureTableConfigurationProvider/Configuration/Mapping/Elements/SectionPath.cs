namespace AzureTable.Provider.Configuration.Mapping.Elements;

internal sealed class SectionPath
{
    public string Path { get; }

    private SectionPath(string path)
        => Path = string.IsNullOrWhiteSpace(path) ? string.Empty : path;

    public static SectionPath Root { get; } = new(string.Empty);

    public SectionPath AddSegment(string segment)
    {
        segment = segment?.Trim() ?? string.Empty;
        ArgumentException.ThrowIfNullOrWhiteSpace(segment);

        return new(string.IsNullOrWhiteSpace(Path) ? segment : $"{Path}:{segment}");
    }

}
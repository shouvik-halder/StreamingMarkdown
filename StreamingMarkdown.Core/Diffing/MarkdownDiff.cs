using StreamingMarkdown.Core.Models.Blocks;

namespace StreamingMarkdown.Core.Diffing;

public sealed class MarkdownDiff
{
    public IReadOnlyList<MarkdownChange> Changes { get; }

    public bool HasChanges =>
        Changes.Count > 0;

    public MarkdownDiff(
        IReadOnlyList<MarkdownChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        Changes = changes;
    }

    public static MarkdownDiff Empty =>
        new([]);
}
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Results;

public sealed class MarkdownUpdate
{
    public MarkdownDocument Document { get; }

    public MarkdownDiff Diff { get; }

    public bool IsCompleted { get; }

    public long Version { get; }

    public MarkdownUpdate(
        MarkdownDocument document,
        MarkdownDiff diff,
        bool isCompleted,
        long version)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diff);

        Document = document;
        Diff = diff;
        IsCompleted = isCompleted;
        Version = version;
    }
}
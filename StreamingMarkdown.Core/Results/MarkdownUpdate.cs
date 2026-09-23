using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Results;

public sealed class MarkdownUpdate
{
    public MarkdownDocument Document { get; }

    public MarkdownDiff Diff { get; }

    public MarkdownStreamResult Result { get; }

    public long Version { get; }

    public bool IsCompleted =>
        Result == MarkdownStreamResult.Completed;

    public MarkdownUpdate(
        MarkdownDocument document,
        MarkdownDiff diff,
        MarkdownStreamResult result,
        long version)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diff);

        Document = document;
        Diff = diff;
        Result = result;
        Version = version;
    }
}
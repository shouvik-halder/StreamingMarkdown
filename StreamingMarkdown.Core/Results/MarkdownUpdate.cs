using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Results;

namespace StreamingMarkdown.Core.Results;

public sealed class MarkdownUpdate
{
    public MarkdownDocument Document { get; }

    public MarkdownDiff Diff { get; }

    public MarkdownStreamResult Result { get; }

    public long Version { get; }

    public string? ErrorMessage { get; }

    public bool IsCompleted =>
        Result == MarkdownStreamResult.Completed;

    public bool HasError =>
        Result == MarkdownStreamResult.Failed;

    public MarkdownUpdate(
        MarkdownDocument document,
        MarkdownDiff diff,
        MarkdownStreamResult result,
        long version,
        string? errorMessage = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(diff);

        Document = document;
        Diff = diff;
        Result = result;
        Version = version;
        ErrorMessage = errorMessage;
    }
}
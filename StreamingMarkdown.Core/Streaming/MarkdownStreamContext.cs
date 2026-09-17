using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Streaming;

public sealed class MarkdownStreamContext
{
    public MarkdownStreamState State { get; internal set; }

    public long Version { get; internal set; }

    public MarkdownDocument Document { get; internal set; }

    public MarkdownStreamContext()
    {
        State = MarkdownStreamState.Idle;
        Version = 0;
        Document = MarkdownDocument.Empty;
    }
}
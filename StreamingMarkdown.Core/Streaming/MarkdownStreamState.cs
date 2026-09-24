namespace StreamingMarkdown.Core.Streaming;

public enum MarkdownStreamState
{
    Idle,
    Streaming,
    Completed,
    Failed,
    Cancelled
}
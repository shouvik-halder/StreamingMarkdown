
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Maui.Streaming;

public sealed class StreamingMarkdownService
    : IStreamingMarkdownService
{
    public IMarkdownStreamSession CreateSession()
    {
        var scheduler = MarkdownStreamScheduler.Create();

        return new MarkdownStreamSession(scheduler);
    }
}
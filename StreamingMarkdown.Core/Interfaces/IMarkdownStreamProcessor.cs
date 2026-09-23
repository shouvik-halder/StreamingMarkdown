using StreamingMarkdown.Core.Results;

namespace StreamingMarkdown.Core.Interfaces;

public interface IMarkdownStreamProcessor
{
    MarkdownUpdate Begin();

    MarkdownUpdate Append(
        string chunk);

    MarkdownUpdate Complete();

    MarkdownUpdate Cancel();

    void Reset();
}
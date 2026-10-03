
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Results;

namespace StreamingMarkdown.Maui.Streaming;

public interface IMarkdownStreamSession : IAsyncDisposable
{
    event Action<MarkdownUpdate>? UpdateAvailable;
    
    MarkdownUpdate? LatestUpdate { get; }

    void Append(string chunk);

    Task CompleteAsync(
        CancellationToken cancellationToken = default);

    Task CancelAsync(
        CancellationToken cancellationToken = default);
}
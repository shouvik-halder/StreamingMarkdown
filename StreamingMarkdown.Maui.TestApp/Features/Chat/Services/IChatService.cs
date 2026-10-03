namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

public interface IChatService
{
    IAsyncEnumerable<string> StreamResponseAsync(
        string message,
        CancellationToken cancellationToken = default);
}
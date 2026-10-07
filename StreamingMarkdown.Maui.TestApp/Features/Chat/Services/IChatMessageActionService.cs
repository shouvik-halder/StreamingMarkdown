
using StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

public interface IChatMessageActionService
{
    Task CopyAsync(
        string text,
        CancellationToken cancellationToken = default);

    Task SetFeedbackAsync(
        Guid messageId,
        ChatMessageFeedback feedback,
        CancellationToken cancellationToken = default);
}

using System.Collections.Concurrent;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

public sealed class LocalChatMessageActionService
    : IChatMessageActionService
{
    private readonly ConcurrentDictionary<Guid, ChatMessageFeedback>
        _feedback = new();

    public async Task CopyAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(text);
        cancellationToken.ThrowIfCancellationRequested();

        await Clipboard.Default.SetTextAsync(text);
    }

    public Task SetFeedbackAsync(
        Guid messageId,
        ChatMessageFeedback feedback,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID cannot be empty.",
                nameof(messageId));
        }

        if (feedback == ChatMessageFeedback.None)
        {
            _feedback.TryRemove(messageId, out _);
        }
        else
        {
            _feedback[messageId] = feedback;
        }

        return Task.CompletedTask;
    }
}
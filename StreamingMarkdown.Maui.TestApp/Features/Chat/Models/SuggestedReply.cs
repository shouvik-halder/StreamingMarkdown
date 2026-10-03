namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

public sealed record SuggestedReply(
    string Text,
    string? Value = null);
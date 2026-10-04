using StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Selectors;

public sealed class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserMessageTemplate { get; set; }
    public DataTemplate? AssistantMessageTemplate { get; set; }
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if(item is not ChatMessageViewModel message)
        {
            throw new ArgumentException($"Expected {nameof(ChatMessageViewModel)}.", nameof(item));
        }
        return message.Role switch 
        {
            ChatMessageRole.User=>UserMessageTemplate ?? throw new InvalidOperationException("UserMessageTemplate has not been configured."),
            ChatMessageRole.Assistant=>AssistantMessageTemplate ?? throw new InvalidOperationException("AssistantMessageTemplate has not been configured."),
            _ => throw new ArgumentOutOfRangeException(nameof(message.Role))
        };
    }
}
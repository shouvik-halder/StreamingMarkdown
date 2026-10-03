using StreamingMarkdown.Maui.TestApp.Features.Chat.ViewModels;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Views;

public partial class ChatPage : ContentPage
{
    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
using System.Collections.Specialized;
using StreamingMarkdown.Maui.TestApp.Features.Chat.ViewModels;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Views;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel vm;
    private bool isSubscribed;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();

        vm = viewModel;
        BindingContext = vm;

        SubscribeToSuggestedReplies();
        RenderSuggestedReplies();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        SubscribeToSuggestedReplies();
        RenderSuggestedReplies();
    }

    protected override void OnDisappearing()
    {
        UnsubscribeFromSuggestedReplies();
        base.OnDisappearing();
    }

    private void SubscribeToSuggestedReplies()
    {
        if (isSubscribed)
            return;

        vm.SuggestedReplies.CollectionChanged += SuggestedRepliesChanged;
        isSubscribed = true;
    }

    private void UnsubscribeFromSuggestedReplies()
    {
        if (!isSubscribed)
            return;

        vm.SuggestedReplies.CollectionChanged -= SuggestedRepliesChanged;
        isSubscribed = false;
    }

    private void SuggestedRepliesChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RenderSuggestedReplies);
    }

    private void RenderSuggestedReplies()
    {
        SuggestedRepliesLayout.Children.Clear();

        foreach (var reply in vm.SuggestedReplies)
        {
            var button = new Button
            {
                Text = reply.Text,
                Command = vm.SuggestedReplyCommand,
                CommandParameter = reply, FontFamily="RobotoSemiBold",
                BackgroundColor = Colors.White,
                BorderColor = Color.FromArgb("#006BD3"),
                BorderWidth = 1,
                CornerRadius = 8,
                TextColor = Color.FromArgb("#006BD3"),
                Padding = new Thickness(12, 8),
                Margin = new Thickness(4)
            };

            SuggestedRepliesLayout.Children.Add(button);
        }
    }

}
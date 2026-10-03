using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StreamingMarkdown.Maui.Streaming;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Models;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.ViewModels;

public sealed class ChatViewModel :
    INotifyPropertyChanged, IAsyncDisposable
{
    private readonly IChatService _chatService;
    private readonly IStreamingMarkdownService _markdownService;

    private CancellationTokenSource? _responseCts;
    private ChatMessageViewModel? _activeAssistantMessage;
    private bool _isBusy;
    private bool _isDisposed;
    private string _inputText = string.Empty;
    private string _status = "Ready";
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (!SetProperty(ref _errorMessage, value))
                return;

            OnPropertyChanged(nameof(HasError));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

    public string InputText
    {
        get => _inputText;
        set
        {
            if (!SetProperty(ref _inputText, value))
                return;

            ((Command)SendCommand).ChangeCanExecute();
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            OnPropertyChanged(nameof(IsNotBusy));
            ((Command)SendCommand).ChangeCanExecute();
            ((Command)CancelCommand).ChangeCanExecute();
            ((Command<SuggestedReply>)SuggestedReplyCommand).ChangeCanExecute();
        }
    }

    public bool IsNotBusy => !IsBusy;

    public ICommand SendCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SuggestedReplyCommand { get; }

    public ChatViewModel(
        IChatService chatService,
        IStreamingMarkdownService markdownService)
    {
        _chatService = chatService;
        _markdownService = markdownService;

        SendCommand = new Command(
            async () => await SendAsync(),
            () => !IsBusy && !string.IsNullOrWhiteSpace(InputText));

        CancelCommand = new Command(
            CancelResponse,
            () => IsBusy);

        SuggestedReplyCommand = new Command<SuggestedReply>(
            async reply =>
            {
                if (reply is null || IsBusy)
                    return;

                await SendAsync(reply.Value ?? reply.Text);
            },
            reply => reply is not null && !IsBusy);
    }

    private async Task SendAsync(string? suppliedText = null)
    {
        if (_isDisposed || IsBusy)
            return;

        var messageText = (suppliedText ?? InputText).Trim();

        if (string.IsNullOrWhiteSpace(messageText))
            return;

        // Remove previous suggestions when the conversation continues.
        foreach (var message in Messages.Where(m => m.IsAssistant))
            message.SuggestedReplies.Clear();

        InputText = string.Empty;

        Messages.Add(new ChatMessageViewModel(
    ChatMessageRole.User,
    SuggestedReplyCommand)
        {
            Text = messageText
        });

        var assistantMessage =
    new ChatMessageViewModel(
        ChatMessageRole.Assistant,
        SuggestedReplyCommand)
    {
        IsStreaming = true
    };

        assistantMessage.MarkdownSession =
            _markdownService.CreateSession();

        Messages.Add(assistantMessage);
        _activeAssistantMessage = assistantMessage;

        _responseCts = new CancellationTokenSource();
        var token = _responseCts.Token;

        IsBusy = true;
        Status = "Assistant is responding...";

        try
        {
            await foreach (var chunk in _chatService
                .StreamResponseAsync(messageText, token)
                .WithCancellation(token))
            {
                token.ThrowIfCancellationRequested();

                // Append through the session API; MarkdownView observes
                // the session and handles rendering.
                await MainThread.InvokeOnMainThreadAsync(
                    () => assistantMessage.MarkdownSession!.Append(chunk));
            }

            token.ThrowIfCancellationRequested();

            await assistantMessage.MarkdownSession!.CompleteAsync(token);

            AddSuggestedReplies(assistantMessage, messageText);

            Status = "Response complete";
        }
        catch (OperationCanceledException)
        {
            try
            {
                await assistantMessage.MarkdownSession!.CancelAsync();
            }
            catch (Exception)
            {
                // Preserve the cancellation outcome if the session
                // has already entered a terminal state.
            }

            Status = "Response cancelled";
        }
        catch (Exception ex)
        {
            assistantMessage.ErrorMessage =
                "The response could not be completed. Please try again.";

            try
            {
                await assistantMessage.MarkdownSession!.CancelAsync();
            }
            catch (Exception)
            {
                // Keep the original response error.
            }

            Status = $"Response failed: {ex.Message}";
        }
        finally
        {
            assistantMessage.IsStreaming = false;
            _activeAssistantMessage = null;

            _responseCts?.Dispose();
            _responseCts = null;

            IsBusy = false;
        }
    }

    private static void AddSuggestedReplies(
        ChatMessageViewModel message,
        string userMessage)
    {
        message.SuggestedReplies.Add(
            new SuggestedReply("Show an example", "Show me an example"));

        message.SuggestedReplies.Add(
            new SuggestedReply("Explain the architecture", "Explain the architecture"));

        message.SuggestedReplies.Add(
            new SuggestedReply("Ask another question", "Tell me more"));
    }

    private void CancelResponse()
    {
        if (!IsBusy)
            return;

        Status = "Cancelling response...";
        _responseCts?.Cancel();
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this, new PropertyChangedEventArgs(propertyName));
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _responseCts?.Cancel();

        // Do not dispose a session while the response producer is still
        // appending to it. Page-level lifecycle coordination will be added
        // when we wire up navigation and teardown.
        foreach (var message in Messages)
        {
            if (message.MarkdownSession is not null &&
                !ReferenceEquals(message, _activeAssistantMessage))
            {
                await message.DisposeAsync();
            }
        }
    }
}
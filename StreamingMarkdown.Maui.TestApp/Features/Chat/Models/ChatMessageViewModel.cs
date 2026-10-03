using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StreamingMarkdown.Maui.Streaming;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

public sealed class ChatMessageViewModel :
    INotifyPropertyChanged, IAsyncDisposable
{
    private string _text = string.Empty;
    private string? _errorMessage;
    private bool _isStreaming;
    private IMarkdownStreamSession? _markdownSession;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; } = Guid.NewGuid();

    public ChatMessageRole Role { get; }

    public bool IsUser => Role == ChatMessageRole.User;
    public bool IsAssistant => Role == ChatMessageRole.Assistant;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

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

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool IsStreaming
    {
        get => _isStreaming;
        set => SetProperty(ref _isStreaming, value);
    }

    public IMarkdownStreamSession? MarkdownSession
    {
        get => _markdownSession;
        set => SetProperty(ref _markdownSession, value);
    }

    public ObservableCollection<SuggestedReply> SuggestedReplies { get; } = [];

    // Command used by the buttons inside this message.
    public ICommand? SuggestedReplyCommand { get; }

    public ChatMessageViewModel(
        ChatMessageRole role,
        ICommand? suggestedReplyCommand = null)
    {
        Role = role;
        SuggestedReplyCommand = suggestedReplyCommand;
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));

        return true;
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    public async ValueTask DisposeAsync()
    {
        var session = MarkdownSession;
        MarkdownSession = null;

        if (session is not null)
            await session.DisposeAsync();
    }
}
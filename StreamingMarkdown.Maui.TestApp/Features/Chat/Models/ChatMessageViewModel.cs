
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StreamingMarkdown.Maui.Streaming;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

public sealed class ChatMessageViewModel :
    INotifyPropertyChanged, IAsyncDisposable
{
    private readonly IChatMessageActionService _actionService;

    private string _text = string.Empty;
    private string? _errorMessage;
    private bool _isStreaming;
    private IMarkdownStreamSession? _markdownSession;
    private ChatMessageFeedback _feedback;

    private readonly Command _copyCommand;
    private readonly Command _positiveFeedbackCommand;
    private readonly Command _negativeFeedbackCommand;
    private readonly Command _feedbackCommand;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; } = Guid.NewGuid();

    public ChatMessageRole Role { get; }

    public bool IsUser => Role == ChatMessageRole.User;

    public bool IsAssistant => Role == ChatMessageRole.Assistant;

    public string Text
    {
        get => _text;
        set
        {
            if (!SetProperty(ref _text, value))
                return;

            _copyCommand.ChangeCanExecute();
        }
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
        set
        {
            if (!SetProperty(ref _isStreaming, value))
                return;

            _copyCommand.ChangeCanExecute();
        }
    }

    public IMarkdownStreamSession? MarkdownSession
    {
        get => _markdownSession;
        set => SetProperty(ref _markdownSession, value);
    }

    public ChatMessageFeedback Feedback
    {
        get => _feedback;
        private set
        {
            if (!SetProperty(ref _feedback, value))
                return;

            OnPropertyChanged(nameof(IsPositiveFeedback));
            OnPropertyChanged(nameof(IsNegativeFeedback));
        }
    }

    public bool IsPositiveFeedback =>
        Feedback == ChatMessageFeedback.Positive;

    public bool IsNegativeFeedback =>
        Feedback == ChatMessageFeedback.Negative;

    public ICommand CopyCommand => _copyCommand;

    public ICommand PositiveFeedbackCommand =>
        _positiveFeedbackCommand;
    public ICommand FeedbackCommand => _feedbackCommand;

    public ICommand NegativeFeedbackCommand =>
        _negativeFeedbackCommand;

    public ChatMessageViewModel(
        ChatMessageRole role,
        IChatMessageActionService actionService)
    {
        Role = role;

        _actionService = actionService
            ?? throw new ArgumentNullException(nameof(actionService));

        _copyCommand = new Command(
            async () => await CopyAsync(),
            CanCopy);

        _positiveFeedbackCommand = new Command(
            async () => await SetFeedbackAsync(
                ChatMessageFeedback.Positive),
            CanGiveFeedback);

        _negativeFeedbackCommand = new Command(
            async () => await SetFeedbackAsync(
                ChatMessageFeedback.Negative),
            CanGiveFeedback);

        _feedbackCommand = new Command(
            async (obj) =>
            {
                if (obj is not string value ||
                    !Enum.TryParse<ChatMessageFeedback>(
                        value, out var feedback))
                {
                    return;
                }

                await SetFeedbackAsync(feedback);
            },
            _ => IsAssistant
        );
    }

    private bool CanCopy() =>
        IsAssistant &&
        !IsStreaming &&
        !string.IsNullOrWhiteSpace(Text);

    private bool CanGiveFeedback() => IsAssistant;

    private async Task CopyAsync()
    {
        if (!CanCopy())
            return;

        await _actionService.CopyAsync(Text);
    }

    private async Task SetFeedbackAsync(
        ChatMessageFeedback requestedFeedback)
    {
        if (!CanGiveFeedback())
            return;

        // Selecting the active option again clears the selection.
        var newFeedback = Feedback == requestedFeedback
            ? ChatMessageFeedback.None
            : requestedFeedback;

        await _actionService.SetFeedbackAsync(Id, newFeedback);

        Feedback = newFeedback;
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
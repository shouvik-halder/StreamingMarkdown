using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StreamingMarkdown.Maui.Streaming;

namespace StreamingMarkdown.Maui.TestApp.ViewModels;

public sealed class StreamingMarkdownTestViewModel
    : INotifyPropertyChanged, IAsyncDisposable
{
    private readonly IStreamingMarkdownService _markdownService;

    private IMarkdownStreamSession? _session;
    private CancellationTokenSource? _streamCts;
    private bool _isStreaming;
    private string _status = "Ready to test.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public IMarkdownStreamSession? MarkdownSession
    {
        get => _session;
        private set
        {
            if (ReferenceEquals(_session, value))
                return;

            _session = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;

            _status = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }

    public StreamingMarkdownTestViewModel(
        IStreamingMarkdownService markdownService)
    {
        _markdownService = markdownService;

        StartCommand = new Command(
            async () => await StartAsync(),
            () => !_isStreaming);

        CancelCommand = new Command(
            async () => await CancelAsync(),
            () => _isStreaming);
    }

    private async Task StartAsync()
    {
        if (_isStreaming)
            return;

        _isStreaming = true;
        UpdateCommands();

        try
        {
            await DisposeCurrentSessionAsync();

            _streamCts = new CancellationTokenSource();
            var token = _streamCts.Token;

            MarkdownSession = _markdownService.CreateSession();

            Status = "Streaming Markdown...";

            const string markdown = """
                # Streaming Markdown

                This content is being appended **incrementally**.

                ## Formatting

                - **Bold text**
                - *Italic text*
                - A normal paragraph with a [sample link](https://example.com).

                ## Blockquote

                > This quote should render as a Markdown blockquote.

                ## Final section

                The stream has reached the end.
                """;

            const int chunkSize = 8;

            for (var i = 0; i < markdown.Length; i += chunkSize)
            {
                token.ThrowIfCancellationRequested();

                var length = Math.Min(
                    chunkSize,
                    markdown.Length - i);

                MarkdownSession.Append(
                    markdown.Substring(i, length));

                await Task.Delay(20, token);
            }

            await MarkdownSession.CompleteAsync(token);

            Status = "Stream completed successfully.";
        }
        catch (OperationCanceledException)
        {
            Status = "Stream cancelled.";
        }
        catch (Exception ex)
        {
            Status = $"Stream failed: {ex.Message}";
        }
        finally
        {
            _streamCts?.Dispose();
            _streamCts = null;

            _isStreaming = false;
            UpdateCommands();
        }
    }

    private async Task CancelAsync()
    {
        if (!_isStreaming || _session is null)
            return;

        Status = "Cancelling stream...";

        // Stop the producer before cancelling the session.
        _streamCts?.Cancel();

        try
        {
            await _session.CancelAsync();
        }
        catch (Exception ex)
        {
            Status = $"Cancellation failed: {ex.Message}";
        }
    }

    private async Task DisposeCurrentSessionAsync()
    {
        var previous = _session;

        if (previous is null)
            return;

        MarkdownSession = null;
        await previous.DisposeAsync();
    }

    private void UpdateCommands()
    {
        ((Command)StartCommand).ChangeCanExecute();
        ((Command)CancelCommand).ChangeCanExecute();
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
        _streamCts?.Cancel();

        if (_session is not null)
        {
            await _session.DisposeAsync();
            _session = null;
        }

        _streamCts?.Dispose();
        _streamCts = null;
    }
}
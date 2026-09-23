using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Interfaces;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Results;

namespace StreamingMarkdown.Core.Streaming;

public sealed class MarkdownStreamProcessor
    : IMarkdownStreamProcessor
{
    private readonly MarkdownBuffer _buffer;
    private readonly IMarkdownParser _parser;
    private readonly IDocumentReconciler _reconciler;
    private readonly IDocumentDiffEngine _diffEngine;
    private readonly MarkdownStreamContext _context;

    public MarkdownStreamProcessor(
        MarkdownBuffer buffer,
        IMarkdownParser parser,
        IDocumentReconciler reconciler,
        IDocumentDiffEngine diffEngine)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(reconciler);
        ArgumentNullException.ThrowIfNull(diffEngine);

        _buffer = buffer;
        _parser = parser;
        _reconciler = reconciler;
        _diffEngine = diffEngine;

        _context = new MarkdownStreamContext();
    }

    public MarkdownUpdate Begin()
    {
        if (_context.State == MarkdownStreamState.Streaming)
        {
            throw new InvalidOperationException(
                "The Markdown stream is already active.");
        }

        _buffer.Clear();

        _context.State =
            MarkdownStreamState.Streaming;

        _context.Version++;

        _context.Document =
            MarkdownDocument.Empty;

        return new MarkdownUpdate(
    _context.Document,
    MarkdownDiff.Empty,
    MarkdownStreamResult.Streaming,
    _context.Version);
    }

    public MarkdownUpdate Append(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        EnsureStreaming();

        if (chunk.Length == 0)
        {
            return new MarkdownUpdate(
                _context.Document,
                MarkdownDiff.Empty,
                MarkdownStreamResult.Streaming,
                _context.Version);
        }

        _buffer.Append(chunk);

        var parsedDocument =
            _parser.Parse(
                _buffer.Content);

        var reconciledDocument =
            _reconciler.Reconcile(
                _context.Document,
                parsedDocument);

        var diff =
            _diffEngine.Compare(
                _context.Document,
                reconciledDocument);

        _context.Document =
            reconciledDocument;

        _context.Version++;

        return new MarkdownUpdate(
    reconciledDocument,
    diff,
    MarkdownStreamResult.Streaming,
    _context.Version);
    }

    public MarkdownUpdate Complete()
    {
        EnsureStreaming();

        var parsedDocument =
            _parser.Parse(
                _buffer.Content);

        var reconciledDocument =
            _reconciler.Reconcile(
                _context.Document,
                parsedDocument);

        var diff =
            _diffEngine.Compare(
                _context.Document,
                reconciledDocument);

        _context.Document =
            reconciledDocument;

        _context.State =
            MarkdownStreamState.Completed;

        _context.Version++;

        return new MarkdownUpdate(
    reconciledDocument,
    diff,
    MarkdownStreamResult.Completed,
    _context.Version);
    }

    public void Reset()
    {
        _buffer.Clear();

        _context.State =
            MarkdownStreamState.Idle;

        _context.Version = 0;

        _context.Document =
            MarkdownDocument.Empty;
    }

public MarkdownUpdate Cancel()
{
    EnsureStreaming();

    _context.State =
        MarkdownStreamState.Cancelled;

    _context.Version++;

    return new MarkdownUpdate(
        _context.Document,
        MarkdownDiff.Empty,
        MarkdownStreamResult.Cancelled,
        _context.Version);
}

    private void EnsureStreaming()
    {
        if (_context.State !=
            MarkdownStreamState.Streaming)
        {
            throw new InvalidOperationException(
                "The Markdown stream is not active.");
        }
    }
}
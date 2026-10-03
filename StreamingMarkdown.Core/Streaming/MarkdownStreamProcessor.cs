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
    private readonly IIncrementalMarkdownParser _incrementalParser;
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

        _incrementalParser =
            new IncrementalMarkdownParser(parser);

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

    var totalStopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    var previousBlockCount =
        _context.Document.Blocks.Count;

    // --------------------------------------------------
    // 1. Append
    // --------------------------------------------------
    var appendStopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    _buffer.Append(chunk);

    appendStopwatch.Stop();

    // --------------------------------------------------
    // 2. Parse
    // --------------------------------------------------
    var reparseStart =
        _context.Document.Blocks.Count > 0
            ? _context.Document.Blocks[^1].SourceStart
            : 0;

    var suffix =
        _buffer.GetSuffix(reparseStart);

    var parseStopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    var parseResult =
        _incrementalParser.ParseSuffix(
            suffix,
            _context.Document,
            reparseStart);

    parseStopwatch.Stop();

    // --------------------------------------------------
    // 3. Reconcile
    // --------------------------------------------------
    var reconcileStopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    var reconciledDocument =
        _reconciler.ReconcileIncremental(
            _context.Document,
            parseResult.Document,
            parseResult.ReusedBlockCount);

    reconcileStopwatch.Stop();

    // --------------------------------------------------
    // 4. Diff
    // --------------------------------------------------
    var diffStopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    var diff =
        _diffEngine.CompareIncremental(
            _context.Document,
            reconciledDocument,
            parseResult.ReusedBlockCount);

    diffStopwatch.Stop();

    // --------------------------------------------------
    // 5. Update context
    // --------------------------------------------------
    _context.Document =
        reconciledDocument;

    _context.Version++;

    totalStopwatch.Stop();

    // --------------------------------------------------
    // Diagnostics
    // --------------------------------------------------
    System.Diagnostics.Debug.WriteLine(
        $"[MarkdownProcessor] " +
        $"Version={_context.Version} | " +
        $"ChunkChars={chunk.Length} | " +
        $"BufferChars={_buffer.Content.Length} | " +
        $"PreviousBlocks={previousBlockCount} | " +
        $"CurrentBlocks={reconciledDocument.Blocks.Count} | " +
        $"ReparseStart={reparseStart} | " +
        $"SuffixChars={suffix.Length} | " +
        $"ReusedBlocks={parseResult.ReusedBlockCount} | " +
        $"DiffChanges={diff.Changes.Count} | " +
        $"Append={appendStopwatch.Elapsed.TotalMilliseconds:F3} ms | " +
        $"Parse={parseStopwatch.Elapsed.TotalMilliseconds:F3} ms | " +
        $"Reconcile={reconcileStopwatch.Elapsed.TotalMilliseconds:F3} ms | " +
        $"Diff={diffStopwatch.Elapsed.TotalMilliseconds:F3} ms | " +
        $"Total={totalStopwatch.Elapsed.TotalMilliseconds:F3} ms");

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

    public MarkdownUpdate Fail(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        EnsureStreaming();

        _context.State =
            MarkdownStreamState.Failed;

        _context.Version++;

        return new MarkdownUpdate(
            MarkdownDocument.Empty,
            MarkdownDiff.Empty,
            MarkdownStreamResult.Failed,
            _context.Version,
            exception.Message);
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
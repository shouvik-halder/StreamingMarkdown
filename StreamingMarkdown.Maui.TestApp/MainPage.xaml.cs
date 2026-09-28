// using StreamingMarkdown.Core.Models;
// using StreamingMarkdown.Core.Models.Blocks;
// using StreamingMarkdown.Core.Models.Inlines;
// using StreamingMarkdown.Maui.Controls;

// namespace StreamingMarkdown.Maui.TestApp;

// public partial class MainPage : ContentPage
// {
//     public MainPage()
//     {
//         InitializeComponent();

//         var document =
//             new MarkdownDocument(
//                 new MarkdownBlock[]
//                 {
//                     new ParagraphBlock(
//                         0,
//                         new MarkdownInline[]
//                         {
//                             new TextInline(
//                                 "Hello from StreamingMarkdown!")
//                         })
//                 });

//         Content =
//             new MarkdownView
//             {
//                 Document = document
//             };
//     }
// }

// using StreamingMarkdown.Core.Parsing;
// using StreamingMarkdown.Maui.Controls;

// namespace StreamingMarkdown.Maui.TestApp;

// public partial class MainPage : ContentPage
// {
//     public MainPage()
//     {
//         InitializeComponent();

//         var markdown = """
// # Streaming Markdown

// This is a **bold** word and this is *italic*.

// Visit [OpenAI](https://openai.com).

// ## Unordered List

// - Apple
// - Banana
// - **Orange**

// ## Ordered List

// 1. First item
// 2. Second item
// 3. Third item

// > This is a quote.
// >
// > It contains **bold text**.
// """;
//         var parser =
//             new MarkdigMarkdownParser();

//         var document =
//             parser.Parse(markdown);

//         Content =
//             new MarkdownView
//             {
//                 Document = document
//             };
//     }
// }

using System.Diagnostics;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Core.Streaming;
using StreamingMarkdown.Maui.Controls;

namespace StreamingMarkdown.Maui.TestApp;

public partial class MainPage : ContentPage
{
    private readonly MarkdownStreamProcessor _processor;
    private readonly MarkdownView _markdownView;

    public MainPage()
    {
        InitializeComponent();

        _processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                new MarkdigMarkdownParser(),
                new DocumentReconciler(),
                new DocumentDiffEngine());

        _markdownView =
            new MarkdownView();

        Content = _markdownView;

        StartStreaming();
    }
private async void StartStreaming()
{
    var scheduler =
        new MarkdownStreamScheduler(
            _processor);

    var uiUpdateCount = 0;

    var markdownViewLock =
        new object();

    var totalUiTime =
        TimeSpan.Zero;

    scheduler.UpdateAvailable +=
        update =>
        {
            MainThread.BeginInvokeOnMainThread(
                () =>
                {
                    var uiStopwatch =
                        System.Diagnostics.Stopwatch.StartNew();

                    _markdownView.ApplyUpdate(
                        update);

                    uiStopwatch.Stop();

                    Interlocked.Increment(
                        ref uiUpdateCount);

                    lock (markdownViewLock)
                    {
                        totalUiTime +=
                            uiStopwatch.Elapsed;
                    }
                });
        };

    var beginUpdate =
        scheduler.Begin();

    _markdownView.ApplyUpdate(
        beginUpdate);

    // --------------------------------------------------
    // Finite, realistic Markdown response
    // --------------------------------------------------

var baseMarkdown =
    """
    # Streaming Markdown Performance Test

    This is a realistic **AI-generated streaming response** used to measure
    incremental parsing, document reconciliation, diffing, scheduling, and
    MAUI rendering performance.

    ## Overview

    The renderer receives Markdown progressively instead of receiving the
    complete document at once. Each event contains a small portion of the
    response, and the scheduler combines pending events before processing.

    The renderer should preserve previously rendered blocks whenever possible
    and update only the portion of the document that has changed.

    ## Features

    - Incremental Markdown parsing
    - Stable document reconciliation
    - Incremental document diffing
    - View reuse
    - Append-only rendering
    - Modified block updates
    - Structural change handling
    - Scheduler-based chunk coalescing

    ## Formatting

    This paragraph contains **bold text**, *italic text*, and
    [a hyperlink](https://example.com).

    > This is a block quote containing **formatted text** and additional
    > content that exercises the quote renderer.

    ## Processing Model

    The streaming pipeline receives chunks from an external source.

    Each chunk is placed into the scheduler queue. The scheduler periodically
    combines pending chunks into a single processing batch. The processor then
    updates the Markdown document and produces a Markdown update.

    The MAUI layer receives the update and applies only the required changes
    to the existing visual tree.

    ## Lists

    - First item
    - Second item
    - Third item
    - Fourth item
    - Fifth item

    ### Ordered Items

    1. Parse incoming Markdown
    2. Reconcile the document
    3. Calculate the document diff
    4. Apply the visual changes
    5. Continue receiving the stream

    ## Performance

    The purpose of this test is to determine whether processing time and
    UI rendering time scale reasonably as the document grows.

    A production AI response can contain headings, paragraphs, lists,
    emphasis, links, and quotations. The benchmark therefore uses a mixture
    of these structures instead of repeatedly appending the same block.

    ## Streaming Behaviour

    During streaming, an incomplete paragraph may be modified many times
    before the response reaches a stable state.

    Previously rendered blocks should remain intact whenever possible.
    Modified blocks should reuse their existing MAUI views rather than
    creating new views unnecessarily.

    Append-only additions should also avoid unnecessary layout reordering.

    ## Additional Content

    Streaming Markdown is particularly useful for AI-generated responses
    because text arrives progressively. A renderer that rebuilds the entire
    visual tree for every incoming event can become increasingly expensive.

    Incremental rendering instead attempts to keep the existing visual tree
    stable and update only the affected portions.

    The scheduler should be able to receive many small chunks while keeping
    the number of UI updates under control.

    The renderer should remain responsive while the response is arriving.
    The important measurement is not simply how quickly one chunk is parsed,
    but whether the complete pipeline can continuously accept incoming data
    without creating an ever-growing processing backlog.

    ## Conclusion

    This benchmark measures a finite Markdown response streamed progressively.
    It does not artificially duplicate the same blocks thousands of times.

    The goal is to verify that the implementation can sustain a realistic
    streaming workload while maintaining responsive MAUI rendering.
    """;

var markdown =
    string.Join(
        "\n\n",
        baseMarkdown,
        baseMarkdown,
        baseMarkdown,
        baseMarkdown);
    // --------------------------------------------------
    // Split response into streaming chunks
    // --------------------------------------------------

    const int chunkSize = 200;

    var chunks =
        new List<string>();

    for (var offset = 0;
         offset < markdown.Length;
         offset += chunkSize)
    {
        var length =
            Math.Min(
                chunkSize,
                markdown.Length - offset);

        chunks.Add(
            markdown.Substring(
                offset,
                length));
    }

    // --------------------------------------------------
    // Simulate 100 chunks/sec
    // --------------------------------------------------

    const int chunksPerSecond = 100;

    var interval =
        TimeSpan.FromMilliseconds(
            1000.0 / chunksPerSecond);

    Debug.WriteLine(
        $"[LoadTest] MarkdownLength={markdown.Length}");

    Debug.WriteLine(
        $"[LoadTest] Chunks={chunks.Count}");

    Debug.WriteLine(
        $"[LoadTest] ChunkSize={chunkSize}");

    Debug.WriteLine(
        $"[LoadTest] TargetRate={chunksPerSecond} chunks/sec");

    // --------------------------------------------------
    // Start timing
    // --------------------------------------------------

    var stopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    // --------------------------------------------------
    // Stream finite response
    // --------------------------------------------------

    foreach (var chunk in chunks)
    {
        scheduler.Append(chunk);

        await Task.Delay(interval);
    }

    // --------------------------------------------------
    // Complete stream
    // --------------------------------------------------

    await scheduler.CompleteAsync();

    // Make sure all UI callbacks have completed.
    await MainThread.InvokeOnMainThreadAsync(
        () => { });

    stopwatch.Stop();

    // --------------------------------------------------
    // Results
    // --------------------------------------------------

    TimeSpan measuredUiTime;

    lock (markdownViewLock)
    {
        measuredUiTime =
            totalUiTime;
    }

    Debug.WriteLine(
        $"[LoadTest] Duration=" +
        $"{stopwatch.Elapsed.TotalSeconds:F2} sec");

    Debug.WriteLine(
        $"[LoadTest] Batches=" +
        $"{scheduler.ProcessedBatchCount}");

    Debug.WriteLine(
        $"[LoadTest] ChunksPerBatch=" +
        $"{(double)chunks.Count /
          scheduler.ProcessedBatchCount:F2}");

    Debug.WriteLine(
        $"[LoadTest] UIUpdates=" +
        $"{uiUpdateCount}");

    Debug.WriteLine(
        $"[LoadTest] TotalUIApplyTime=" +
        $"{measuredUiTime.TotalSeconds:F2} sec");

    Debug.WriteLine(
        $"[LoadTest] AverageUIApplyTime=" +
        $"{(uiUpdateCount == 0
            ? 0
            : measuredUiTime.TotalMilliseconds /
              uiUpdateCount):F2} ms");

    Debug.WriteLine(
        $"[LoadTest] TotalCoreProcessingTime=" +
        $"{scheduler.TotalProcessingTime.TotalSeconds:F4} sec");

    Debug.WriteLine(
        $"[LoadTest] AverageCoreProcessingTime=" +
        $"{(scheduler.ProcessedBatchCount == 0
            ? 0
            : scheduler.TotalProcessingTime.TotalMilliseconds /
              scheduler.ProcessedBatchCount):F2} ms");

    Debug.WriteLine(
        $"[LoadTest] ProcessedChunks=" +
        $"{scheduler.ProcessedChunkCount}");
}

}

// using System.Diagnostics;
// using StreamingMarkdown.Core.Diffing;
// using StreamingMarkdown.Core.Parsing;
// using StreamingMarkdown.Core.Streaming;
// using StreamingMarkdown.Maui.Controls;

// namespace StreamingMarkdown.Maui.TestApp;

// public partial class MainPage : ContentPage
// {
//     private readonly MarkdownStreamProcessor _processor;
//     private readonly MarkdownView _markdownView;

//     public MainPage()
//     {
//         InitializeComponent();

//         _processor =
//             new MarkdownStreamProcessor(
//                 new MarkdownBuffer(),
//                 new MarkdigMarkdownParser(),
//                 new DocumentReconciler(),
//                 new DocumentDiffEngine());

//         _markdownView =
//             new MarkdownView();

//         Content =
//             _markdownView;

//         TestQuotes();
//     }

//     private void TestViewReuse()
// {
//     _processor.Begin();

//     var firstUpdate =
//         _processor.Append(
//             "First paragraph\n\nSecond paragraph");

//     _markdownView.ApplyUpdate(
//         firstUpdate);

//     var layout =
//         GetLayout();

//     Assert(
//         layout.Children.Count == 2,
//         "Expected two rendered blocks.");

//     var firstView =
//         layout.Children[0];

//     var secondView =
//         layout.Children[1];

//     var secondUpdate =
//         _processor.Append(
//             "!");

//     _markdownView.ApplyUpdate(
//         secondUpdate);

//     var firstViewAfter =
//         layout.Children[0];

//     var secondViewAfter =
//         layout.Children[1];

//     Assert(
//         ReferenceEquals(
//             firstView,
//             firstViewAfter),
//         "Unchanged first block should reuse its View.");

//     Assert(
//     ReferenceEquals(
//         secondView,
//         secondViewAfter),
//     "Modified block should reuse its existing View.");

//     System.Diagnostics.Debug.WriteLine(
//         "View reuse test passed.");
// }

// private void TestHeadingViewReuse()
// {
//     _processor.Begin();

//     var firstUpdate =
//         _processor.Append(
//             "# Hello");

//     _markdownView.ApplyUpdate(
//         firstUpdate);

//     var layout =
//         GetLayout();

//     Assert(
//         layout.Children.Count == 1,
//         "Expected one rendered block.");

//     var firstView =
//         layout.Children[0];

//     var secondUpdate =
//         _processor.Append(
//             " World");

//     _markdownView.ApplyUpdate(
//         secondUpdate);

//     var secondView =
//         layout.Children[0];

//     Assert(
//         ReferenceEquals(
//             firstView,
//             secondView),
//         "Modified heading should reuse its existing View.");

//     Debug.WriteLine(
//         "Heading view reuse test passed.");
// }

// private void TestLists()
// {
//     var markdown = """
// # Unordered List

// - Apple
// - **Banana**
// - *Orange*
// - [OpenAI](https://openai.com)

// # Ordered List

// 1. First item
// 2. **Second item**
// 3. *Third item*
// 4. [OpenAI](https://openai.com)
// """;

//     var parser =
//         new MarkdigMarkdownParser();

//     var document =
//         parser.Parse(markdown);

//     _markdownView.Document =
//         document;
// }    

// private void TestListViewReuse()
// {
//     _processor.Begin();

//     var firstUpdate =
//         _processor.Append(
//             "- First\n- Sec");

//     _markdownView.ApplyUpdate(
//         firstUpdate);

//     var layout =
//         GetLayout();

//     Assert(
//         layout.Children.Count == 1,
//         "Expected one rendered list block.");

//     var firstView =
//         layout.Children[0];

//     var secondUpdate =
//         _processor.Append(
//             "ond\n- Third");

//     _markdownView.ApplyUpdate(
//         secondUpdate);

//     var secondView =
//         layout.Children[0];

//     Assert(
//         ReferenceEquals(
//             firstView,
//             secondView),
//         "Modified list should reuse its existing View.");

//     Assert(
//         layout.Children.Count == 1,
//         "Expected one list block after update.");

//     Debug.WriteLine(
//         "List view reuse test passed.");
// }

// private void TestQuotes()
// {
//     var markdown = """
// # Quotes

// > This is a simple quote.

// > This quote contains **bold text**.

// > This quote contains *italic text*.

// > Visit [OpenAI](https://openai.com).
// """;

//     var parser =
//         new MarkdigMarkdownParser();

//     var document =
//         parser.Parse(markdown);

//     _markdownView.Document =
//         document;
// }







// private VerticalStackLayout GetLayout()
// {
//     var scrollView =
//         (ScrollView)_markdownView.Content!;

//     return (VerticalStackLayout)
//         scrollView.Content!;
// }

// private static void Assert(
//     bool condition,
//     string message)
// {
//     if (!condition)
//     {
//         throw new InvalidOperationException(
//             message);
//     }
// }
// }
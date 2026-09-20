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
//             new ScrollView
//             {
//                 Content =
//                     _markdownView
//             };

//         StartStreaming();
//     }

// private async void StartStreaming()
// {
//     _processor.Begin();

//     var chunks = new[]
//     {
//         "# Streaming ",
//         "Markdown\n\n",
//         "This is **",
//         "streaming ",
//         "Markdown** ",
//         "rendered ",
//         "inside ",
//         "MAUI."
//     };

//     foreach (var chunk in chunks)
//     {
//         var update =
//             _processor.Append(chunk);

//         _markdownView.ApplyUpdate(
//             update);

//         await Task.Delay(500);
//     }

//     var finalUpdate =
//         _processor.Complete();

//     _markdownView.ApplyUpdate(
//         finalUpdate);
// }

//     private async void StartStreaming()
// {
//     _processor.Begin();

//     var chunks = new[]
//     {
//         "# Hello\n\n",
//         "This is ",
//         "**streaming** ",
//         "Markdown.\n\n",
//         "- First\n",
//         "- Second\n",
//         "- Third"
//     };

//     foreach (var chunk in chunks)
//     {
//         var update =
//             _processor.Append(chunk);

//         _markdownView.ApplyUpdate(
//             update);

//         await Task.Delay(7);
//     }

//     var finalUpdate =
//         _processor.Complete();

//     _markdownView.ApplyUpdate(
//         finalUpdate);
// }

// }

using System.Diagnostics;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
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

        Content =
            _markdownView;

        TestQuotes();
    }

    private void TestViewReuse()
{
    _processor.Begin();

    var firstUpdate =
        _processor.Append(
            "First paragraph\n\nSecond paragraph");

    _markdownView.ApplyUpdate(
        firstUpdate);

    var layout =
        GetLayout();

    Assert(
        layout.Children.Count == 2,
        "Expected two rendered blocks.");

    var firstView =
        layout.Children[0];

    var secondView =
        layout.Children[1];

    var secondUpdate =
        _processor.Append(
            "!");

    _markdownView.ApplyUpdate(
        secondUpdate);

    var firstViewAfter =
        layout.Children[0];

    var secondViewAfter =
        layout.Children[1];

    Assert(
        ReferenceEquals(
            firstView,
            firstViewAfter),
        "Unchanged first block should reuse its View.");

    Assert(
    ReferenceEquals(
        secondView,
        secondViewAfter),
    "Modified block should reuse its existing View.");

    System.Diagnostics.Debug.WriteLine(
        "View reuse test passed.");
}

private void TestHeadingViewReuse()
{
    _processor.Begin();

    var firstUpdate =
        _processor.Append(
            "# Hello");

    _markdownView.ApplyUpdate(
        firstUpdate);

    var layout =
        GetLayout();

    Assert(
        layout.Children.Count == 1,
        "Expected one rendered block.");

    var firstView =
        layout.Children[0];

    var secondUpdate =
        _processor.Append(
            " World");

    _markdownView.ApplyUpdate(
        secondUpdate);

    var secondView =
        layout.Children[0];

    Assert(
        ReferenceEquals(
            firstView,
            secondView),
        "Modified heading should reuse its existing View.");

    Debug.WriteLine(
        "Heading view reuse test passed.");
}

private void TestLists()
{
    var markdown = """
# Unordered List

- Apple
- **Banana**
- *Orange*
- [OpenAI](https://openai.com)

# Ordered List

1. First item
2. **Second item**
3. *Third item*
4. [OpenAI](https://openai.com)
""";

    var parser =
        new MarkdigMarkdownParser();

    var document =
        parser.Parse(markdown);

    _markdownView.Document =
        document;
}    

private void TestListViewReuse()
{
    _processor.Begin();

    var firstUpdate =
        _processor.Append(
            "- First\n- Sec");

    _markdownView.ApplyUpdate(
        firstUpdate);

    var layout =
        GetLayout();

    Assert(
        layout.Children.Count == 1,
        "Expected one rendered list block.");

    var firstView =
        layout.Children[0];

    var secondUpdate =
        _processor.Append(
            "ond\n- Third");

    _markdownView.ApplyUpdate(
        secondUpdate);

    var secondView =
        layout.Children[0];

    Assert(
        ReferenceEquals(
            firstView,
            secondView),
        "Modified list should reuse its existing View.");

    Assert(
        layout.Children.Count == 1,
        "Expected one list block after update.");

    Debug.WriteLine(
        "List view reuse test passed.");
}

private void TestQuotes()
{
    var markdown = """
# Quotes

> This is a simple quote.

> This quote contains **bold text**.

> This quote contains *italic text*.

> Visit [OpenAI](https://openai.com).
""";

    var parser =
        new MarkdigMarkdownParser();

    var document =
        parser.Parse(markdown);

    _markdownView.Document =
        document;
}







private VerticalStackLayout GetLayout()
{
    var scrollView =
        (ScrollView)_markdownView.Content!;

    return (VerticalStackLayout)
        scrollView.Content!;
}

private static void Assert(
    bool condition,
    string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(
            message);
    }
}
}
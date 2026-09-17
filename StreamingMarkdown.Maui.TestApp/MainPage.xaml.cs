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

        TestViewReuse();
    }

    private void TestViewReuse()
    {
        _processor.Begin();

        var firstUpdate =
            _processor.Append(
                "First paragraph\n\nSecond paragraph");

        _markdownView.ApplyUpdate(
            firstUpdate);

        var secondUpdate =
    _processor.Append(
        "!\n\nThird paragraph");

_markdownView.ApplyUpdate(
    secondUpdate);

        var scrollView =
            (ScrollView)_markdownView.Content!;

        var layout =
            (VerticalStackLayout)scrollView.Content!;

        var firstView =
            layout.Children[0];

        var secondView =
            layout.Children[1];

            var firstViewAfter =
    layout.Children[0];

var secondViewAfter =
    layout.Children[1];

var thirdView =
    layout.Children[2];

    Debug.Assert(
    ReferenceEquals(
        firstView,
        firstViewAfter));

        Debug.Assert(
    !ReferenceEquals(
        secondView,
        secondViewAfter));

        Debug.Assert(
    thirdView is Label);

        System.Diagnostics.Debug.WriteLine(
            $"Initial views: {layout.Children.Count}");

        // More tests here...
    }
}
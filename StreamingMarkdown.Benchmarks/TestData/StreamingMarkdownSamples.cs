namespace StreamingMarkdown.Benchmarks.TestData;

public static class StreamingMarkdownSamples
{
    public const string SimpleMarkdown =
        """
        # Streaming Markdown

        This is a simple paragraph that is being streamed into the renderer.

        This is another paragraph with some additional text.
        """;

    public const string MarkdownHeavy =
        """
        # Streaming Markdown

        This is **bold text** and this is *italic text*.

        Here is a [link](https://example.com).

        ## Features

        - First item
        - Second item
        - **Third item**
        - *Fourth item*

        ## Ordered List

        1. First item
        2. Second item
        3. Third item

        > This is a quote.

        > This quote contains **bold text**.

        Another paragraph follows the quote.
        """;
}
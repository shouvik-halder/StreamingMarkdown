namespace StreamingMarkdown.Maui.Configuration;

using StreamingMarkdown.Maui.Styling;

internal static class MarkdownStyleFactory
{
    public static MarkdownStyle Create(
        StreamingMarkdownOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return MarkdownStyleResolver.Resolve(
            options,
            inlineStyle: null);
    }
}
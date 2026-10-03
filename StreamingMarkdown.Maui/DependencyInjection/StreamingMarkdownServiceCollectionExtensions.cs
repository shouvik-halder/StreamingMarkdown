using Microsoft.Extensions.DependencyInjection;
using StreamingMarkdown.Maui.Configuration;
using StreamingMarkdown.Maui.Streaming;

namespace StreamingMarkdown.Maui.DependencyInjection;

public static class StreamingMarkdownServiceCollectionExtensions
{
    public static MauiAppBuilder UseStreamingMarkdown(
        this MauiAppBuilder builder,
        Action<StreamingMarkdownOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new StreamingMarkdownOptions();
        configure?.Invoke(options);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IStreamingMarkdownService,StreamingMarkdownService>();

        return builder;
    }
}
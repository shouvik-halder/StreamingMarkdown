using System.Runtime.CompilerServices;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Services;

public sealed class MockChatService : IChatService
{
    public async IAsyncEnumerable<string> StreamResponseAsync(
        string message,
        [EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        var response = GetResponse(message);

        const int chunkSize = 12;

        for (var index = 0; index < response.Length; index += chunkSize)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var length = Math.Min(
                chunkSize,
                response.Length - index);

            yield return response.Substring(index, length);

            await Task.Delay(25, cancellationToken);
        }
    }

    private static string GetResponse(string message)
    {
        if (message.Contains(
                "example",
                StringComparison.OrdinalIgnoreCase))
        {
            return """
                ## Example: streaming Markdown

                The response arrives in **small chunks**.

                - The service produces text.
                - The ViewModel appends each chunk.
                - `MarkdownView` renders the evolving response.

                You can continue the conversation using the suggested replies below.
                """;
        }

        if (message.Contains(
                "architecture",
                StringComparison.OrdinalIgnoreCase))
        {
            return """
                ## Streaming architecture

                1. The user sends a message.
                2. The chat service streams response chunks.
                3. The ViewModel appends each chunk to a dedicated Markdown session.
                4. The assistant message updates progressively.

                This keeps the chat UI independent of the AI provider.
                """;
        }

        return """
            I've received your message.

            This is a simulated assistant response, streamed progressively through your
            existing **StreamingMarkdown** renderer.

            What would you like to explore next?
            """;
    }
}
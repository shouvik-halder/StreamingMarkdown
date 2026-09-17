using StreamingMarkdown.Core.Models;

namespace StreamingMarkdown.Core.Diffing;

public interface IDocumentReconciler
{
    MarkdownDocument Reconcile(
        MarkdownDocument previous,
        MarkdownDocument current);
}
using System.Diagnostics;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Maui.Rendering;
using StreamingMarkdown.Maui.Styling;

namespace StreamingMarkdown.Maui.Controls;

public sealed class MarkdownView : ContentView
{
    private readonly VerticalStackLayout _layout;
    private readonly MarkdownViewState _state;

    private bool _isApplyingUpdate;

    public static readonly BindableProperty DocumentProperty =
        BindableProperty.Create(
            nameof(Document),
            typeof(MarkdownDocument),
            typeof(MarkdownView),
            MarkdownDocument.Empty,
            propertyChanged:
                OnDocumentChanged);

    public static new readonly BindableProperty StyleProperty =
        BindableProperty.Create(
            nameof(Style),
            typeof(MarkdownStyle),
            typeof(MarkdownView),
            null,
            propertyChanged:
                OnStyleChanged);

    public MarkdownDocument Document
    {
        get =>
            (MarkdownDocument)GetValue(
                DocumentProperty);

        set =>
            SetValue(
                DocumentProperty,
                value);
    }

    public new MarkdownStyle? Style
    {
        get =>
            (MarkdownStyle?)GetValue(
                StyleProperty);

        set =>
            SetValue(
                StyleProperty,
                value);
    }

    public MarkdownView()
    {
        _layout =
            new VerticalStackLayout
            {
                Spacing = 0
            };

        _state =
            new MarkdownViewState();

        Content =
            new ScrollView
            {
                Content = _layout
            };
    }

    private static void OnDocumentChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        var view =
            (MarkdownView)bindable;

        if (view._isApplyingUpdate)
        {
            return;
        }

        view.RenderDocument(
            (MarkdownDocument)newValue);
    }

    private static void OnStyleChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        var view =
            (MarkdownView)bindable;

        view.RenderDocument(
            view.Document);
    }

    private void RenderDocument(
        MarkdownDocument document)
    {
        _layout.Children.Clear();

        _state.Clear();

        var renderer =
            new MarkdownDocumentRenderer(
                Style);

        foreach (var block in document.Blocks)
        {
            var blockView =
                renderer.RenderBlock(
                    block);

            if (blockView is null)
            {
                continue;
            }

            _state.SetView(
                block,
                blockView);

            _layout.Children.Add(
                blockView.View);
        }
    }

    public void ApplyUpdate(
        MarkdownUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var totalStopwatch =
            Stopwatch.StartNew();

        var addedCount = 0;
        var removedCount = 0;
        var modifiedCount = 0;

        var addedTime =
            TimeSpan.Zero;

        var removedTime =
            TimeSpan.Zero;

        var modifiedTime =
            TimeSpan.Zero;

        var reorderTime =
            TimeSpan.Zero;

        var reorderPerformed = false;

        _isApplyingUpdate = true;

        try
        {
            if (!update.Diff.HasChanges)
            {
                SetValue(
                    DocumentProperty,
                    update.Document);

                return;
            }

            var renderer =
                new MarkdownDocumentRenderer(
                    Style);

            foreach (var change in update.Diff.Changes)
            {
                switch (change.Type)
                {
                    case MarkdownChangeType.Added:
                    {
                        addedCount++;

                        var stopwatch =
                            Stopwatch.StartNew();

                        ApplyAdded(
                            change,
                            renderer);

                        stopwatch.Stop();

                        addedTime +=
                            stopwatch.Elapsed;

                        break;
                    }

                    case MarkdownChangeType.Removed:
                    {
                        removedCount++;

                        var stopwatch =
                            Stopwatch.StartNew();

                        ApplyRemoved(
                            change);

                        stopwatch.Stop();

                        removedTime +=
                            stopwatch.Elapsed;

                        break;
                    }

                    case MarkdownChangeType.Modified:
                    {
                        modifiedCount++;

                        var stopwatch =
                            Stopwatch.StartNew();

                        ApplyModified(
                            change);

                        stopwatch.Stop();

                        modifiedTime +=
                            stopwatch.Elapsed;

                        break;
                    }
                }
            }

            var requiresReorder =
                update.Diff.Changes.Any(
                    change =>
                        change.Type !=
                            MarkdownChangeType.Modified &&
                        !IsAppendOnlyAddition(
                            change));

            if (requiresReorder)
            {
                reorderPerformed = true;

                var reorderStopwatch =
                    Stopwatch.StartNew();

                ReorderViews(
                    update.Document);

                reorderStopwatch.Stop();

                reorderTime =
                    reorderStopwatch.Elapsed;
            }

            SetValue(
                DocumentProperty,
                update.Document);
        }
        finally
        {
            _isApplyingUpdate = false;

            totalStopwatch.Stop();

            Debug.WriteLine(
                $"[MarkdownView] ApplyUpdate " +
                $"Total={totalStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
                $"Added={addedCount} " +
                $"({addedTime.TotalMilliseconds:F2} ms), " +
                $"Modified={modifiedCount} " +
                $"({modifiedTime.TotalMilliseconds:F2} ms), " +
                $"Removed={removedCount} " +
                $"({removedTime.TotalMilliseconds:F2} ms), " +
                $"Reorder={reorderPerformed} " +
                $"({reorderTime.TotalMilliseconds:F2} ms)");
        }
    }

    private void ApplyAdded(
        MarkdownChange change,
        MarkdownDocumentRenderer renderer)
    {
        if (change.Current is null)
        {
            return;
        }

        var totalStopwatch =
            Stopwatch.StartNew();

        var renderStopwatch =
            Stopwatch.StartNew();

        var blockView =
            renderer.RenderBlock(
                change.Current);

        renderStopwatch.Stop();

        if (blockView is null)
        {
            return;
        }

        var stateStopwatch =
            Stopwatch.StartNew();

        _state.SetView(
            change.Current,
            blockView);

        stateStopwatch.Stop();

        var layoutStopwatch =
            Stopwatch.StartNew();

        if (change.CurrentIndex ==
            _layout.Children.Count)
        {
            _layout.Children.Add(
                blockView.View);
        }

        layoutStopwatch.Stop();

        totalStopwatch.Stop();

        Debug.WriteLine(
            $"[MarkdownView] ApplyAdded " +
            $"BlockType={change.Current.GetType().Name}, " +
            $"Render={renderStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"State={stateStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"LayoutAdd={layoutStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"Total={totalStopwatch.Elapsed.TotalMilliseconds:F2} ms");
    }

    private bool IsAppendOnlyAddition(
        MarkdownChange change)
    {
        return change.Type ==
               MarkdownChangeType.Added &&
               change.CurrentIndex ==
               _layout.Children.Count;
    }

    private void ApplyRemoved(
        MarkdownChange change)
    {
        if (change.Previous is null)
        {
            return;
        }

        if (!_state.TryGetView(
                change.Previous.Id,
                out var blockView))
        {
            return;
        }

        _layout.Children.Remove(
            blockView.View);

        _state.RemoveView(
            change.Previous.Id);
    }

    private void ApplyModified(
        MarkdownChange change)
    {
        if (change.Current is null)
        {
            return;
        }

        if (!_state.TryGetView(
                change.Current.Id,
                out var blockView))
        {
            var renderer =
                new MarkdownDocumentRenderer(
                    Style);

            ApplyAdded(
                change,
                renderer);

            return;
        }

        blockView.Update(
            change.Current);
    }

    private void ReorderViews(
        MarkdownDocument document)
    {
        var indexLookups = 0;
        var moves = 0;

        for (var targetIndex = 0;
             targetIndex < document.Blocks.Count;
             targetIndex++)
        {
            var block =
                document.Blocks[targetIndex];

            if (!_state.TryGetView(
                    block.Id,
                    out var blockView))
            {
                continue;
            }

            if (blockView is null)
            {
                continue;
            }

            var view =
                blockView.View;

            indexLookups++;

            var currentIndex =
                _layout.Children.IndexOf(
                    view);

            if (currentIndex ==
                targetIndex)
            {
                continue;
            }

            if (currentIndex >= 0)
            {
                _layout.Children.RemoveAt(
                    currentIndex);
            }

            _layout.Children.Insert(
                targetIndex,
                view);

            moves++;
        }

        Debug.WriteLine(
            $"[MarkdownView] ReorderViews " +
            $"Blocks={document.Blocks.Count}, " +
            $"IndexLookups={indexLookups}, " +
            $"Moves={moves}");
    }
}
using System.Diagnostics;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Maui.Rendering;
using StreamingMarkdown.Maui.Styling;
using Microsoft.Maui.ApplicationModel;
using StreamingMarkdown.Maui.Streaming;
using StreamingMarkdown.Maui.Configuration;

namespace StreamingMarkdown.Maui.Controls;

public sealed class MarkdownView : ContentView
{
    private readonly VerticalStackLayout _layout;
    private readonly MarkdownViewState _state;

    private readonly StreamingMarkdownOptions _options;
    private bool _isApplyingUpdate;

    public static readonly BindableProperty DocumentProperty =
        BindableProperty.Create(
            nameof(Document),
            typeof(MarkdownDocument),
            typeof(MarkdownView),
            MarkdownDocument.Empty,
            propertyChanged:
                OnDocumentChanged);

    public static readonly BindableProperty MarkdownStyleProperty =
    BindableProperty.Create(
        nameof(MarkdownStyle),
        typeof(MarkdownStyle),
        typeof(MarkdownView),
        defaultValue: null,
        propertyChanged: OnMarkdownStyleChanged);
    
    public static readonly BindableProperty SourceProperty =
    BindableProperty.Create(
        nameof(Source),
        typeof(IMarkdownStreamSession),
        typeof(MarkdownView),
        default(IMarkdownStreamSession),
        propertyChanged: OnSourceChanged);

public IMarkdownStreamSession? Source
{
    get => (IMarkdownStreamSession?)GetValue(SourceProperty);
    set => SetValue(SourceProperty, value);
}

private Action<MarkdownUpdate>? _sourceUpdateHandler;

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

private static void OnSourceChanged(
    BindableObject bindable,
    object oldValue,
    object newValue)
{
    var view = (MarkdownView)bindable;

    if (oldValue is IMarkdownStreamSession oldSession &&
        view._sourceUpdateHandler is not null)
    {
        oldSession.UpdateAvailable -= view._sourceUpdateHandler;
    }

    view._sourceUpdateHandler = null;

    // Clear content from the previously recycled message.
    view.Document = MarkdownDocument.Empty;

    if (newValue is not IMarkdownStreamSession newSession)
        return;

    Action<MarkdownUpdate> handler = update =>
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Ignore updates belonging to another recycled cell.
            if (!ReferenceEquals(view.Source, newSession))
                return;

            view.ApplyUpdate(update);
        });
    };

    view._sourceUpdateHandler = handler;
    newSession.UpdateAvailable += handler;

    // Rehydrate a recycled MarkdownView from the
    // session's latest known document.
    var latestUpdate = newSession.LatestUpdate;

if (latestUpdate is not null)
{
    // Prevent the Document property callback from
    // rendering the document a second time.
    view._isApplyingUpdate = true;

    try
    {
        view.Document = latestUpdate.Document;
    }
    finally
    {
        view._isApplyingUpdate = false;
    }

    view.RenderDocument(latestUpdate.Document);
}
}
    
    public MarkdownStyle? MarkdownStyle
{
    get => (MarkdownStyle?)GetValue(MarkdownStyleProperty);
    set => SetValue(MarkdownStyleProperty, value);
}

    public MarkdownView()
{
    _options =
        Application.Current?.Handler?.MauiContext?.Services
            .GetService<StreamingMarkdownOptions>()
        ?? new StreamingMarkdownOptions();

    _layout = new VerticalStackLayout { Spacing = 0 };
    _state = new MarkdownViewState();

    Content = _layout;
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

    private static void OnMarkdownStyleChanged(
    BindableObject bindable,
    object oldValue,
    object newValue)
{
    var view = (MarkdownView)bindable;

    view.RenderDocument(view.Document);
}

    private void RenderDocument(
        MarkdownDocument document)
    {
        _layout.Children.Clear();

        _state.Clear();

        var renderer = new MarkdownDocumentRenderer(_options, MarkdownStyle);

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

            var renderer = new MarkdownDocumentRenderer(_options, MarkdownStyle);

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
#if DEBUG
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
#endif
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
#if DEBUG
        Debug.WriteLine(
            $"[MarkdownView] ApplyAdded " +
            $"BlockType={change.Current.GetType().Name}, " +
            $"Render={renderStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"State={stateStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"LayoutAdd={layoutStopwatch.Elapsed.TotalMilliseconds:F2} ms, " +
            $"Total={totalStopwatch.Elapsed.TotalMilliseconds:F2} ms");
#endif
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
            blockView?.View);

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
            var renderer = new MarkdownDocumentRenderer(_options, MarkdownStyle);

            ApplyAdded(
                change,
                renderer);

            return;
        }

        blockView?.Update(
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
#if DEBUG
        Debug.WriteLine(
            $"[MarkdownView] ReorderViews " +
            $"Blocks={document.Blocks.Count}, " +
            $"IndexLookups={indexLookups}, " +
            $"Moves={moves}");
#endif
    }
}
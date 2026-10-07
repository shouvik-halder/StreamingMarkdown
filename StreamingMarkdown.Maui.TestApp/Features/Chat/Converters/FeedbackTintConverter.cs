using System.Globalization;
using StreamingMarkdown.Maui.TestApp.Features.Chat.Models;

namespace StreamingMarkdown.Maui.TestApp.Features.Chat.Converters;

public sealed class FeedbackTintConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        bool isSelected = value is true;
        ChatMessageFeedback feedback = ChatMessageFeedback.None;
        bool isValidFeedback =
            parameter is string feedbackType &&
            Enum.TryParse(
                feedbackType,
                out feedback);

        if (!isSelected || !isValidFeedback)
            return Color.FromArgb("#B0B7BF");

        return feedback == ChatMessageFeedback.Negative
            ? Colors.Red
            : feedback == ChatMessageFeedback.Positive
                ? Color.FromArgb("#006BD3")
                : Color.FromArgb("#B0B7BF");
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
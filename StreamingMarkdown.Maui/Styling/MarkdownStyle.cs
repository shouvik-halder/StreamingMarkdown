namespace StreamingMarkdown.Maui.Styling;

public sealed class MarkdownStyle
{
    private string? _fontFamily;
    private Color? _textColor;
    private double? _bodyFontSize;
    private double? _lineHeight;

    private double? _heading1FontSize;
    private double? _heading2FontSize;
    private double? _heading3FontSize;
    private double? _heading4FontSize;
    private double? _heading5FontSize;
    private double? _heading6FontSize;

    private double? _blockSpacing;
    private double? _listIndent;
    private double? _quoteIndent;

    // Font family
    public bool HasFontFamily { get; private set; }

    public string? FontFamily
    {
        get => _fontFamily;
        set
        {
            _fontFamily = value;
            HasFontFamily = true;
        }
    }

    // Text color
    public bool HasTextColor { get; private set; }

    public Color TextColor
    {
        get => _textColor ?? Color.FromArgb("#1E293B");
        set
        {
            _textColor = value;
            HasTextColor = true;
        }
    }

    // Body font size
    public bool HasBodyFontSize { get; private set; }

    public double BodyFontSize
    {
        get => _bodyFontSize ?? 16;
        set
        {
            _bodyFontSize = value;
            HasBodyFontSize = true;
        }
    }

    // Line height
    public bool HasLineHeight { get; private set; }

    public double LineHeight
    {
        get => _lineHeight ?? 1.0;
        set
        {
            _lineHeight = value;
            HasLineHeight = true;
        }
    }

    // Heading 1
    public bool HasHeading1FontSize { get; private set; }

    public double Heading1FontSize
    {
        get => _heading1FontSize ?? 32;
        set
        {
            _heading1FontSize = value;
            HasHeading1FontSize = true;
        }
    }

    // Heading 2
    public bool HasHeading2FontSize { get; private set; }

    public double Heading2FontSize
    {
        get => _heading2FontSize ?? 28;
        set
        {
            _heading2FontSize = value;
            HasHeading2FontSize = true;
        }
    }

    // Heading 3
    public bool HasHeading3FontSize { get; private set; }

    public double Heading3FontSize
    {
        get => _heading3FontSize ?? 24;
        set
        {
            _heading3FontSize = value;
            HasHeading3FontSize = true;
        }
    }

    // Heading 4
    public bool HasHeading4FontSize { get; private set; }

    public double Heading4FontSize
    {
        get => _heading4FontSize ?? 20;
        set
        {
            _heading4FontSize = value;
            HasHeading4FontSize = true;
        }
    }

    // Heading 5
    public bool HasHeading5FontSize { get; private set; }

    public double Heading5FontSize
    {
        get => _heading5FontSize ?? 18;
        set
        {
            _heading5FontSize = value;
            HasHeading5FontSize = true;
        }
    }

    // Heading 6
    public bool HasHeading6FontSize { get; private set; }

    public double Heading6FontSize
    {
        get => _heading6FontSize ?? 16;
        set
        {
            _heading6FontSize = value;
            HasHeading6FontSize = true;
        }
    }

    // Block spacing
    public bool HasBlockSpacing { get; private set; }

    public double BlockSpacing
    {
        get => _blockSpacing ?? 0;
        set
        {
            _blockSpacing = value;
            HasBlockSpacing = true;
        }
    }

    // List indentation
    public bool HasListIndent { get; private set; }

    public double ListIndent
    {
        get => _listIndent ?? 20;
        set
        {
            _listIndent = value;
            HasListIndent = true;
        }
    }

    // Quote indentation
    public bool HasQuoteIndent { get; private set; }

    public double QuoteIndent
    {
        get => _quoteIndent ?? 16;
        set
        {
            _quoteIndent = value;
            HasQuoteIndent = true;
        }
    }
}
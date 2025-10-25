using System.Windows.Input;

namespace OMS.Components;

public partial class GradientHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(GradientHeader), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(GradientHeader), string.Empty);

    public static readonly BindableProperty TitleAlignmentProperty =
        BindableProperty.Create(nameof(TitleAlignment), typeof(LayoutOptions), typeof(GradientHeader), LayoutOptions.Center);

    public static readonly BindableProperty ActionButtonTextProperty =
        BindableProperty.Create(nameof(ActionButtonText), typeof(string), typeof(GradientHeader), string.Empty);

    public static readonly BindableProperty ActionButtonCommandProperty =
        BindableProperty.Create(nameof(ActionButtonCommand), typeof(ICommand), typeof(GradientHeader), null);

    public static readonly BindableProperty ShowActionButtonProperty =
        BindableProperty.Create(nameof(ShowActionButton), typeof(bool), typeof(GradientHeader), false);

    public static readonly BindableProperty ActionButtonBackgroundProperty =
        BindableProperty.Create(nameof(ActionButtonBackground), typeof(Brush), typeof(GradientHeader), 
            new SolidColorBrush(Colors.Transparent));

    public static readonly BindableProperty ActionButtonBorderColorProperty =
        BindableProperty.Create(nameof(ActionButtonBorderColor), typeof(Color), typeof(GradientHeader), Colors.White);

    public static readonly BindableProperty ActionButtonBorderWidthProperty =
        BindableProperty.Create(nameof(ActionButtonBorderWidth), typeof(double), typeof(GradientHeader), 1.0);

    public static readonly BindableProperty ActionButtonFontSizeProperty =
        BindableProperty.Create(nameof(ActionButtonFontSize), typeof(double), typeof(GradientHeader), 14.0);

    public GradientHeader()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public LayoutOptions TitleAlignment
    {
        get => (LayoutOptions)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public ICommand ActionButtonCommand
    {
        get => (ICommand)GetValue(ActionButtonCommandProperty);
        set => SetValue(ActionButtonCommandProperty, value);
    }

    public bool ShowActionButton
    {
        get => (bool)GetValue(ShowActionButtonProperty);
        set => SetValue(ShowActionButtonProperty, value);
    }

    public Brush ActionButtonBackground
    {
        get => (Brush)GetValue(ActionButtonBackgroundProperty);
        set => SetValue(ActionButtonBackgroundProperty, value);
    }

    public Color ActionButtonBorderColor
    {
        get => (Color)GetValue(ActionButtonBorderColorProperty);
        set => SetValue(ActionButtonBorderColorProperty, value);
    }

    public double ActionButtonBorderWidth
    {
        get => (double)GetValue(ActionButtonBorderWidthProperty);
        set => SetValue(ActionButtonBorderWidthProperty, value);
    }

    public double ActionButtonFontSize
    {
        get => (double)GetValue(ActionButtonFontSizeProperty);
        set => SetValue(ActionButtonFontSizeProperty, value);
    }

    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
}

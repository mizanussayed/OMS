namespace OMS.Components;

public partial class StatsCard : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(StatsCard), string.Empty);

    public static readonly BindableProperty IconBackgroundColorProperty =
        BindableProperty.Create(nameof(IconBackgroundColor), typeof(Color), typeof(StatsCard), Color.FromArgb("#dbeafe"));

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(StatsCard), string.Empty);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(StatsCard), "0");

    public StatsCard()
    {
        InitializeComponent();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public Color IconBackgroundColor
    {
        get => (Color)GetValue(IconBackgroundColorProperty);
        set => SetValue(IconBackgroundColorProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}

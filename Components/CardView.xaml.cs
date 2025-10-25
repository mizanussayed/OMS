namespace OMS.Components;

public partial class CardView : ContentView
{
    public static readonly BindableProperty CardContentProperty =
        BindableProperty.Create(nameof(CardContent), typeof(View), typeof(CardView), null);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(CardView), new CornerRadius(12));

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(CardView), Color.FromArgb("#e5e7eb"));

    public static readonly BindableProperty BorderWidthProperty =
        BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(CardView), 1.0);

    public static readonly BindableProperty ShadowOpacityProperty =
        BindableProperty.Create(nameof(ShadowOpacity), typeof(float), typeof(CardView), 0.1f);

    public static readonly BindableProperty ShadowRadiusProperty =
        BindableProperty.Create(nameof(ShadowRadius), typeof(float), typeof(CardView), 8f);

    public static readonly BindableProperty ShadowOffsetProperty =
        BindableProperty.Create(nameof(ShadowOffset), typeof(Point), typeof(CardView), new Point(0, 2));

    public CardView()
    {
        InitializeComponent();
        BackgroundColor = Colors.White;
        Padding = new Thickness(16);
    }

    public View CardContent
    {
        get => (View)GetValue(CardContentProperty);
        set => SetValue(CardContentProperty, value);
    }

    public CornerRadius CardCornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    public float ShadowOpacity
    {
        get => (float)GetValue(ShadowOpacityProperty);
        set => SetValue(ShadowOpacityProperty, value);
    }

    public float ShadowRadius
    {
        get => (float)GetValue(ShadowRadiusProperty);
        set => SetValue(ShadowRadiusProperty, value);
    }

    public Point ShadowOffset
    {
        get => (Point)GetValue(ShadowOffsetProperty);
        set => SetValue(ShadowOffsetProperty, value);
    }
}

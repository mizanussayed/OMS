namespace OMS.Components;

public partial class GradientProgressBar : ContentView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(GradientProgressBar), 0.0, 
            propertyChanged: OnProgressChanged);

    public static readonly BindableProperty MaxWidthProperty =
        BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(GradientProgressBar), 300.0,
            propertyChanged: OnProgressChanged);

    public static readonly BindableProperty ProgressWidthProperty =
        BindableProperty.Create(nameof(ProgressWidth), typeof(double), typeof(GradientProgressBar), 0.0);

    public static readonly BindableProperty ProgressGradientProperty =
        BindableProperty.Create(nameof(ProgressGradient), typeof(Brush), typeof(GradientProgressBar), null);

    public static readonly BindableProperty UseWarningColorProperty =
        BindableProperty.Create(nameof(UseWarningColor), typeof(bool), typeof(GradientProgressBar), false,
            propertyChanged: OnColorTypeChanged);

    public GradientProgressBar()
    {
        InitializeComponent();
        UpdateGradient();
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    public double ProgressWidth
    {
        get => (double)GetValue(ProgressWidthProperty);
        set => SetValue(ProgressWidthProperty, value);
    }

    public Brush ProgressGradient
    {
        get => (Brush)GetValue(ProgressGradientProperty);
        set => SetValue(ProgressGradientProperty, value);
    }

    public bool UseWarningColor
    {
        get => (bool)GetValue(UseWarningColorProperty);
        set => SetValue(UseWarningColorProperty, value);
    }

    private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is GradientProgressBar progressBar)
        {
            progressBar.ProgressWidth = progressBar.MaxWidth * progressBar.Progress;
        }
    }

    private static void OnColorTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is GradientProgressBar progressBar)
        {
            progressBar.UpdateGradient();
        }
    }

    private void UpdateGradient()
    {
        if (UseWarningColor)
        {
            ProgressGradient = new SolidColorBrush(Color.FromArgb("#f97316")); // Orange500
        }
        else
        {
            // Purple to Pink gradient
            var gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0)
            };
            gradient.GradientStops.Add(new GradientStop(Color.FromArgb("#9333ea"), 0.0f)); // Purple600
            gradient.GradientStops.Add(new GradientStop(Color.FromArgb("#db2777"), 1.0f)); // Pink600
            ProgressGradient = gradient;
        }
    }
}

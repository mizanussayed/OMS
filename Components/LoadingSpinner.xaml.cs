namespace OMS.Components;

public partial class LoadingSpinner : ContentView
{
    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingSpinner), true);

    public static readonly BindableProperty LoadingTextProperty =
        BindableProperty.Create(nameof(LoadingText), typeof(string), typeof(LoadingSpinner), "Loading...", propertyChanged: OnLoadingTextChanged);

    public static readonly BindableProperty SpinnerColorProperty =
        BindableProperty.Create(nameof(SpinnerColor), typeof(Color), typeof(LoadingSpinner), Color.FromArgb("#9333ea"));

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(LoadingSpinner), Color.FromArgb("#6B7280"));

    public static readonly BindableProperty SpinnerSizeProperty =
        BindableProperty.Create(nameof(SpinnerSize), typeof(double), typeof(LoadingSpinner), 40.0);

    public static readonly BindableProperty HasLoadingTextProperty =
        BindableProperty.Create(nameof(HasLoadingText), typeof(bool), typeof(LoadingSpinner), true);

    public LoadingSpinner()
    {
        InitializeComponent();
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string LoadingText
    {
        get => (string)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    public Color SpinnerColor
    {
        get => (Color)GetValue(SpinnerColorProperty);
        set => SetValue(SpinnerColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public double SpinnerSize
    {
        get => (double)GetValue(SpinnerSizeProperty);
        set => SetValue(SpinnerSizeProperty, value);
    }

    public bool HasLoadingText
    {
        get => (bool)GetValue(HasLoadingTextProperty);
        private set => SetValue(HasLoadingTextProperty, value);
    }

    private static void OnLoadingTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LoadingSpinner spinner)
        {
            spinner.HasLoadingText = !string.IsNullOrEmpty(newValue as string);
        }
    }
}

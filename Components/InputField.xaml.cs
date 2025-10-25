namespace OMS.Components;

public partial class InputField : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(InputField), string.Empty, propertyChanged: OnLabelChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(InputField), string.Empty, BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(InputField), string.Empty);

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(InputField), string.Empty, propertyChanged: OnErrorMessageChanged);

    public static readonly BindableProperty HelperTextProperty =
        BindableProperty.Create(nameof(HelperText), typeof(string), typeof(InputField), string.Empty, propertyChanged: OnHelperTextChanged);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(InputField), Keyboard.Default);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(InputField), false);

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(InputField), int.MaxValue);

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(InputField), Color.FromArgb("#E5E7EB"));

    public static readonly BindableProperty HasLabelProperty =
        BindableProperty.Create(nameof(HasLabel), typeof(bool), typeof(InputField), false);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(InputField), false);

    public static readonly BindableProperty HasHelperTextProperty =
        BindableProperty.Create(nameof(HasHelperText), typeof(bool), typeof(InputField), false);

    public event EventHandler<TextChangedEventArgs>? TextChangedEvent;
    public event EventHandler<FocusEventArgs>? FocusedEvent;
    public event EventHandler<FocusEventArgs>? UnfocusedEvent;

    public InputField()
    {
        InitializeComponent();
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public string HelperText
    {
        get => (string)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public bool HasLabel
    {
        get => (bool)GetValue(HasLabelProperty);
        private set => SetValue(HasLabelProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        private set => SetValue(HasErrorProperty, value);
    }

    public bool HasHelperText
    {
        get => (bool)GetValue(HasHelperTextProperty);
        private set => SetValue(HasHelperTextProperty, value);
    }

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InputField field)
        {
            field.HasLabel = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private static void OnErrorMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InputField field)
        {
            field.HasError = !string.IsNullOrEmpty(newValue as string);
            field.BorderColor = field.HasError ? Color.FromArgb("#ea580c") : Color.FromArgb("#E5E7EB");
        }
    }

    private static void OnHelperTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is InputField field)
        {
            field.HasHelperText = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        TextChangedEvent?.Invoke(this, e);
    }

    private void OnFocused(object? sender, FocusEventArgs e)
    {
        if (!HasError)
        {
            BorderColor = Color.FromArgb("#9333ea");
        }
        FocusedEvent?.Invoke(this, e);
    }

    private void OnUnfocused(object? sender, FocusEventArgs e)
    {
        if (!HasError)
        {
            BorderColor = Color.FromArgb("#E5E7EB");
        }
        UnfocusedEvent?.Invoke(this, e);
    }
}

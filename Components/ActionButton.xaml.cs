using System.Windows.Input;

namespace OMS.Components;

public enum ButtonVariant
{
    Primary,
    Secondary,
    Outline,
    Danger,
    Success
}

public partial class ActionButton : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ActionButton), string.Empty);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ActionButton), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(ActionButton), null);

    public static readonly BindableProperty VariantProperty =
        BindableProperty.Create(nameof(Variant), typeof(ButtonVariant), typeof(ActionButton), ButtonVariant.Primary, propertyChanged: OnVariantChanged);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(ActionButton), 14.0);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(ActionButton), FontAttributes.Bold);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(ActionButton), Colors.White);

    public static readonly BindableProperty ButtonBackgroundProperty =
        BindableProperty.Create(nameof(ButtonBackground), typeof(Brush), typeof(ActionButton), Brush.Default);

    public static readonly BindableProperty ButtonBorderColorProperty =
        BindableProperty.Create(nameof(ButtonBorderColor), typeof(Color), typeof(ActionButton), Colors.Transparent);

    public static readonly BindableProperty ButtonBorderWidthProperty =
        BindableProperty.Create(nameof(ButtonBorderWidth), typeof(double), typeof(ActionButton), 0.0);

    public ActionButton()
    {
        InitializeComponent();
        UpdateButtonStyle();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ButtonVariant Variant
    {
        get => (ButtonVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontAttributes FontAttributes
    {
        get => (FontAttributes)GetValue(FontAttributesProperty);
        set => SetValue(FontAttributesProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public Brush ButtonBackground
    {
        get => (Brush)GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    public Color ButtonBorderColor
    {
        get => (Color)GetValue(ButtonBorderColorProperty);
        set => SetValue(ButtonBorderColorProperty, value);
    }

    public double ButtonBorderWidth
    {
        get => (double)GetValue(ButtonBorderWidthProperty);
        set => SetValue(ButtonBorderWidthProperty, value);
    }

    private static void OnVariantChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button)
        {
            button.UpdateButtonStyle();
        }
    }

    private void UpdateButtonStyle()
    {
        switch (Variant)
        {
            case ButtonVariant.Primary:
                TextColor = Colors.White;
                ButtonBackground = Application.Current?.Resources["PurplePinkGradient"] as Brush ?? Brush.Purple;
                ButtonBorderWidth = 0;
                break;

            case ButtonVariant.Secondary:
                TextColor = Color.FromArgb("#374151");
                ButtonBackground = Color.FromArgb("#F3F4F6");
                ButtonBorderWidth = 0;
                break;

            case ButtonVariant.Outline:
                TextColor = Color.FromArgb("#6B7280");
                ButtonBackground = Colors.White;
                ButtonBorderColor = Color.FromArgb("#D1D5DB");
                ButtonBorderWidth = 1;
                break;

            case ButtonVariant.Danger:
                TextColor = Colors.White;
                ButtonBackground = Color.FromArgb("#dc2626");
                ButtonBorderWidth = 0;
                break;

            case ButtonVariant.Success:
                TextColor = Colors.White;
                ButtonBackground = Color.FromArgb("#059669");
                ButtonBorderWidth = 0;
                break;
        }
    }
}

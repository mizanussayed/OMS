using System.Windows.Input;

namespace OMS.Components;

public partial class IconButton : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(IconButton), string.Empty, propertyChanged: OnIconChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(IconButton), string.Empty, propertyChanged: OnTextChanged);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(IconButton), null);

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(IconButton), null);

    public static readonly BindableProperty IconSizeProperty =
        BindableProperty.Create(nameof(IconSize), typeof(double), typeof(IconButton), 20.0);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(IconButton), Colors.Black);

    public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(IconButton), Colors.Black);

    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(IconButton), 14.0);

    public static readonly BindableProperty FontAttributesProperty =
        BindableProperty.Create(nameof(FontAttributes), typeof(FontAttributes), typeof(IconButton), FontAttributes.None);

    public static readonly BindableProperty HasIconProperty =
        BindableProperty.Create(nameof(HasIcon), typeof(bool), typeof(IconButton), false);

    public static readonly BindableProperty HasTextProperty =
        BindableProperty.Create(nameof(HasText), typeof(bool), typeof(IconButton), false);

    public IconButton()
    {
        InitializeComponent();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
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

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Color IconColor
    {
        get => (Color)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public Color TextColor
    {
        get => (Color)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
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

    public bool HasIcon
    {
        get => (bool)GetValue(HasIconProperty);
        private set => SetValue(HasIconProperty, value);
    }

    public bool HasText
    {
        get => (bool)GetValue(HasTextProperty);
        private set => SetValue(HasTextProperty, value);
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is IconButton button)
        {
            button.HasIcon = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is IconButton button)
        {
            button.HasText = !string.IsNullOrEmpty(newValue as string);
        }
    }
}

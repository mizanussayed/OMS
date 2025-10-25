using System.Windows.Input;

namespace OMS.Components;

public partial class EmptyState : ContentView
{
    public static readonly BindableProperty IconProperty =
        BindableProperty.Create(nameof(Icon), typeof(string), typeof(EmptyState), "📦", propertyChanged: OnIconChanged);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnTitleChanged);

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnMessageChanged);

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(EmptyState), string.Empty, propertyChanged: OnActionTextChanged);

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(EmptyState), null);

    public static readonly BindableProperty IconSizeProperty =
        BindableProperty.Create(nameof(IconSize), typeof(double), typeof(EmptyState), 48.0);

    public static readonly BindableProperty IconColorProperty =
        BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(EmptyState), Color.FromArgb("#9CA3AF"));

    public static readonly BindableProperty HasIconProperty =
        BindableProperty.Create(nameof(HasIcon), typeof(bool), typeof(EmptyState), true);

    public static readonly BindableProperty HasTitleProperty =
        BindableProperty.Create(nameof(HasTitle), typeof(bool), typeof(EmptyState), false);

    public static readonly BindableProperty HasMessageProperty =
        BindableProperty.Create(nameof(HasMessage), typeof(bool), typeof(EmptyState), false);

    public static readonly BindableProperty HasActionProperty =
        BindableProperty.Create(nameof(HasAction), typeof(bool), typeof(EmptyState), false);

    public EmptyState()
    {
        InitializeComponent();
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
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

    public bool HasIcon
    {
        get => (bool)GetValue(HasIconProperty);
        private set => SetValue(HasIconProperty, value);
    }

    public bool HasTitle
    {
        get => (bool)GetValue(HasTitleProperty);
        private set => SetValue(HasTitleProperty, value);
    }

    public bool HasMessage
    {
        get => (bool)GetValue(HasMessageProperty);
        private set => SetValue(HasMessageProperty, value);
    }

    public bool HasAction
    {
        get => (bool)GetValue(HasActionProperty);
        private set => SetValue(HasActionProperty, value);
    }

    private static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyState state)
        {
            state.HasIcon = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyState state)
        {
            state.HasTitle = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private static void OnMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyState state)
        {
            state.HasMessage = !string.IsNullOrEmpty(newValue as string);
        }
    }

    private static void OnActionTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EmptyState state)
        {
            state.HasAction = !string.IsNullOrEmpty(newValue as string);
        }
    }
}

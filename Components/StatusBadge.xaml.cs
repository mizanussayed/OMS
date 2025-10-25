namespace OMS.Components;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusBadge), string.Empty);

    public static readonly BindableProperty StatusTypeProperty =
        BindableProperty.Create(nameof(StatusType), typeof(StatusType), typeof(StatusBadge), 
            StatusType.Pending, propertyChanged: OnStatusTypeChanged);

    public static readonly BindableProperty BadgeTextColorProperty =
        BindableProperty.Create(nameof(BadgeTextColor), typeof(Color), typeof(StatusBadge), Colors.Black);

    public static readonly new BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(StatusBadge), Colors.Gray);

    public static readonly BindableProperty BorderColorProperty =
        BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(StatusBadge), Colors.Gray);

    public StatusBadge()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public StatusType StatusType
    {
        get => (StatusType)GetValue(StatusTypeProperty);
        set => SetValue(StatusTypeProperty, value);
    }

    public Color BadgeTextColor
    {
        get => (Color)GetValue(BadgeTextColorProperty);
        set => SetValue(BadgeTextColorProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public Color BorderColor
    {
        get => (Color)GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    private static void OnStatusTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge && newValue is StatusType statusType)
        {
            badge.UpdateColors(statusType);
        }
    }

    private void UpdateColors(StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.Pending:
                BackgroundColor = Color.FromArgb("#fef3c7"); // Yellow100
                BorderColor = Color.FromArgb("#fde68a"); // Yellow200
                BadgeTextColor = Color.FromArgb("#854d0e"); // Yellow800
                break;
            case StatusType.Completed:
                BackgroundColor = Color.FromArgb("#dbeafe"); // Blue100
                BorderColor = Color.FromArgb("#bfdbfe"); // Blue200
                BadgeTextColor = Color.FromArgb("#1e40af"); // Blue800
                break;
            case StatusType.Delivered:
            case StatusType.Success:
                BackgroundColor = Color.FromArgb("#dcfce7"); // Green100
                BorderColor = Color.FromArgb("#bbf7d0"); // Green200
                BadgeTextColor = Color.FromArgb("#166534"); // Green800
                break;
            case StatusType.Warning:
                BackgroundColor = Color.FromArgb("#fff7ed"); // Orange50
                BorderColor = Color.FromArgb("#fed7aa"); // Orange200
                BadgeTextColor = Color.FromArgb("#7c2d12"); // Orange900
                break;
            case StatusType.Error:
                BackgroundColor = Color.FromArgb("#fee2e2"); // Red100
                BorderColor = Color.FromArgb("#fecaca"); // Red200
                BadgeTextColor = Color.FromArgb("#991b1b"); // Red800
                break;
        }
    }
}

public enum StatusType
{
    Pending,
    Completed,
    Delivered,
    Success,
    Warning,
    Error
}

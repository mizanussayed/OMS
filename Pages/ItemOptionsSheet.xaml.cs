namespace OMS.Pages;

public partial class ItemOptionsSheet : ContentPage
{
    public event EventHandler? EditRequested;
    public event EventHandler? DeleteRequested;

    public ItemOptionsSheet()
    {
        InitializeComponent();
        
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        // Add haptic feedback
        try
        {
#if ANDROID || IOS
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
        }
        catch { }
        
        if (Content is Grid grid && grid.Children.Count > 1 && grid.Children[1] is Border border)
        {
            border.TranslationY = 400;           
            await border.TranslateTo(0, 0, 250, Easing.CubicOut);
        }
    }

    private async void OnEditTapped(object? sender, EventArgs e)
    {
        try
        {
#if ANDROID || IOS
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
        }
        catch { }
        
        EditRequested?.Invoke(this, EventArgs.Empty);
        await CloseSheet();
    }

    private async void OnDeleteTapped(object? sender, EventArgs e)
    {
        try
        {
#if ANDROID || IOS
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
#endif
        }
        catch { }
        
        DeleteRequested?.Invoke(this, EventArgs.Empty);
        await CloseSheet();
    }

    private async void OnBackgroundTapped(object? sender, EventArgs e)
    {
        await CloseSheet();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CloseSheet();
    }

    private async Task CloseSheet()
    {
        if (Content is Grid grid && grid.Children.Count > 1 && grid.Children[1] is Border border)
        {
            await border.TranslateTo(0, 400, 200, Easing.CubicIn);
        }
        
        await Shell.Current.Navigation.PopModalAsync(false);
    }
}

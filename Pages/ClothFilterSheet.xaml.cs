namespace OMS.Pages;

public partial class ClothFilterSheet : ContentPage
{
    public event EventHandler<ClothFilterEventArgs>? FiltersApplied;
    
    public List<string> AvailableColors { get; set; } = new();
    
    public ClothFilterSheet(string currentSearch, string currentColor, bool currentLowStock, List<string> colors)
    {
        InitializeComponent();
        
        AvailableColors = colors;
        ColorPicker.ItemsSource = AvailableColors;
        
        SearchEntry.Text = currentSearch;
        ColorPicker.SelectedItem = currentColor;
        LowStockSwitch.IsToggled = currentLowStock;
        
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        try
        {
#if ANDROID || IOS
            HapticFeedback.Default.Perform(HapticFeedbackType.Click);
#endif
        }
        catch { }
        
        FilterSheet.TranslationY = 600;           
        await FilterSheet.TranslateTo(0, 0, 250, Easing.CubicOut);
    }

    private async void OnApplyClicked(object? sender, EventArgs e)
    {
        var args = new ClothFilterEventArgs
        {
            SearchText = SearchEntry.Text ?? string.Empty,
            SelectedColor = ColorPicker.SelectedItem?.ToString() ?? "All",
            IsLowStockOnly = LowStockSwitch.IsToggled
        };
        
        FiltersApplied?.Invoke(this, args);
        await CloseSheet();
    }

    private async void OnClearAllClicked(object? sender, EventArgs e)
    {
        SearchEntry.Text = string.Empty;
        ColorPicker.SelectedItem = "All";
        LowStockSwitch.IsToggled = false;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await CloseSheet();
    }

    private async void OnBackgroundTapped(object? sender, EventArgs e)
    {
        await CloseSheet();
    }

    private async Task CloseSheet()
    {
        await FilterSheet.TranslateTo(0, 600, 200, Easing.CubicIn);
        await Shell.Current.Navigation.PopModalAsync(false);
    }
}

public class ClothFilterEventArgs : EventArgs
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedColor { get; set; } = "All";
    public bool IsLowStockOnly { get; set; }
}

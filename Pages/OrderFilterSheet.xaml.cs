namespace OMS.Pages;

public partial class OrderFilterSheet : ContentPage
{
    public event EventHandler<OrderFilterEventArgs>? FiltersApplied;
    
    public OrderFilterSheet(string currentSearch, string currentStatus)
    {
        InitializeComponent();
        
        StatusPicker.ItemsSource = new List<string> { "All", "Pending", "Completed", "Delivered" };
        
        SearchEntry.Text = currentSearch;
        StatusPicker.SelectedItem = currentStatus;
        
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
        var args = new OrderFilterEventArgs
        {
            SearchText = SearchEntry.Text ?? string.Empty,
            SelectedStatus = StatusPicker.SelectedItem?.ToString() ?? "All"
        };
        
        FiltersApplied?.Invoke(this, args);
        await CloseSheet();
    }

    private async void OnClearAllClicked(object? sender, EventArgs e)
    {
        SearchEntry.Text = string.Empty;
        StatusPicker.SelectedItem = "All";
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

public class OrderFilterEventArgs : EventArgs
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedStatus { get; set; } = "All";
}

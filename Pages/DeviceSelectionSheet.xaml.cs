using Microsoft.Maui.Controls.Shapes;

namespace OMS.Pages;

public partial class DeviceSelectionSheet : ContentPage
{
    public event EventHandler<string>? DeviceSelected;
    private readonly string[] _devices;
    
    public DeviceSelectionSheet(string[] devices)
    {
        InitializeComponent();
        _devices = devices;
        
        this.Loaded += OnLoaded;
        PopulateDevices();
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {   
        DeviceSheet.TranslationY = 600;           
        await DeviceSheet.TranslateToAsync(0, 0, 250, Easing.CubicOut);
    }

    private void PopulateDevices()
    {
        DeviceListContainer.Clear();
        
        foreach (var device in _devices)
        {
            var deviceButton = new Border
            {
                Padding = new Thickness(16, 12),
                BackgroundColor = Color.FromArgb("#f9fafb"),
                StrokeThickness = 1,
                Stroke = Color.FromArgb("#e5e7eb"),
                Margin = new Thickness(0, 0, 0, 8),
                StrokeShape = new RoundRectangle { CornerRadius = 8 }
            };
            
            var gesture = new TapGestureRecognizer();
            gesture.Tapped += async (s, e) =>
            {
                DeviceSelected?.Invoke(this, device);
                await CloseSheet();
            };
            deviceButton.GestureRecognizers.Add(gesture);
            
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 12
            };
            
            var icon = new Label
            {
                Text = "🖨️",
                FontSize = 24,
                VerticalOptions = LayoutOptions.Center
            };
            
            var nameLabel = new Label
            {
                Text = device,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#111827"),
                VerticalOptions = LayoutOptions.Center
            };
            
            grid.Add(icon, 0, 0);
            grid.Add(nameLabel, 1, 0);
            
            deviceButton.Content = grid;
            DeviceListContainer.Add(deviceButton);
        }
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
        await DeviceSheet.TranslateToAsync(0, 600, 200, Easing.CubicIn);
        await Shell.Current.Navigation.PopModalAsync(false);
    }
}

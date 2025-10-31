namespace OMS.Services;

public class AlertService : IAlert
{
    public Task DisplayAlert(string title, string message, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page?.DisplayAlert(title, message, cancel) ?? Task.CompletedTask;
    }

    public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        return page?.DisplayAlert(title, message, accept, cancel) ?? Task.FromResult(false);
    }
}
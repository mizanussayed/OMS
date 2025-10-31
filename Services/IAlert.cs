namespace OMS.Services;

public interface IAlert
{
    Task DisplayAlert(string title, string message, string cancel);
    Task<bool> DisplayAlert(string title, string message, string accept, string cancel);
}
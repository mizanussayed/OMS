using System.Windows.Input;

namespace OMS.Components;

public static class ConfirmDialog
{
    public static async Task<bool> ShowAsync(
        string title,
        string message,
        string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return false;

        return await page.DisplayAlert(title, message, confirmText, cancelText);
    }

    public static async Task ShowMessageAsync(
        string title,
        string message,
        string okText = "OK")
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        await page.DisplayAlert(title, message, okText);
    }

    public static async Task<string?> ShowActionSheetAsync(
        string title,
        string cancelText,
        string? destructionText,
        params string[] buttons)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return null;

        return await page.DisplayActionSheet(title, cancelText, destructionText, buttons);
    }

    public static async Task<string?> ShowPromptAsync(
        string title,
        string message,
        string placeholder = "",
        string initialValue = "",
        string acceptText = "OK",
        string cancelText = "Cancel",
        int maxLength = -1,
        Keyboard? keyboard = null)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return null;

        return await page.DisplayPromptAsync(
            title,
            message,
            acceptText,
            cancelText,
            placeholder,
            maxLength,
            keyboard ?? Keyboard.Default,
            initialValue);
    }
}

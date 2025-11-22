
namespace OMS.Services;

public interface IBluetoothPrinterService
{
    Task<List<string>> GetPairedDevicesAsync();
    Task<bool> ConnectAsync(string deviceName);
    Task<bool> PrintFormattedTextAsync(List<string> lines, int fontSize = 12, bool centerAlign = true, bool isBody = true);
    Task DisconnectAsync();
}

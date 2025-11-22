using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions;

namespace OMS.Services;

public class BluetoothPrinterService : IBluetoothPrinterService
{
    private IBluetoothLE _ble;
    private IAdapter _adapter;
    private IDevice? _connectedDevice;
    private ICharacteristic? _writeCharacteristic;

    private readonly Guid _printerServiceUuid = Guid.Parse("000018f0-0000-1000-8000-00805f9b34fb");
    private readonly Guid _writeCharacteristicUuid = Guid.Parse("00002af1-0000-1000-8000-00805f9b34fb");

    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMilliseconds = 2500;
    private const int DisconnectDelayMilliseconds = 1000;

    private const int DefaultChunkSize = 180; // Optimized for most devices (was 20)
    private int _negotiatedMtu = DefaultChunkSize;
    private bool _useWriteWithoutResponse = false;

    public BluetoothPrinterService()
    {
        _ble = CrossBluetoothLE.Current;
        _adapter = CrossBluetoothLE.Current.Adapter;
    }

    public async Task<List<string>> GetPairedDevicesAsync()
    {
        var devices = new List<string>();

        try
        {
            var systemDevices = _adapter.GetSystemConnectedOrPairedDevices();
            devices.AddRange(systemDevices.Select(d => d.Name ?? "Unknown Device"));
        }
        catch
        {
        }

        return await Task.FromResult(devices);
    }

    public async Task<bool> ConnectAsync(string deviceName)
    {
        try
        {
            if (!_ble.IsOn)
            {
                if (Application.Current!.Windows[0].Page != null)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert("Error", "Bluetooth is turned off", "OK");
                }
                return false;
            }

            var devices = _adapter.GetSystemConnectedOrPairedDevices();
            _connectedDevice = devices.FirstOrDefault(d => d.Name == deviceName);

            if (_connectedDevice == null)
            {
                _adapter.ScanTimeout = 10000;
                _adapter.DeviceDiscovered += (s, a) =>
                {
                    if (a.Device.Name == deviceName)
                        _connectedDevice = a.Device;
                };

                await _adapter.StartScanningForDevicesAsync();
                await _adapter.StopScanningForDevicesAsync();
            }

            if (_connectedDevice == null)
                return false;
            if (_connectedDevice.State == DeviceState.Connected)
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(_connectedDevice);
                    await Task.Delay(DisconnectDelayMilliseconds);
                }
                catch
                {
                }
            }

            bool connected = false;
            Exception? lastException = null;

            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    var connectParameters = new ConnectParameters(forceBleTransport: true, autoConnect: false);
                    await _adapter.ConnectToDeviceAsync(_connectedDevice, connectParameters);
                    await Task.Delay(300);
                    if (_connectedDevice.State == DeviceState.Connected)
                    {
                        connected = true;
                        break;
                    }
                }
                catch (DeviceConnectionException ex)
                {
                    lastException = ex;

                    if (attempt < MaxRetryAttempts)
                    {
                        if (_connectedDevice.State != DeviceState.Disconnected)
                        {
                            await _adapter.DisconnectDeviceAsync(_connectedDevice);
                            await Task.Delay(DisconnectDelayMilliseconds);
                        }
                        await Task.Delay(RetryDelayMilliseconds);
                    }
                }
            }

            if (!connected)
            {
                string errorMessage = "Could not connect to printer. ";

                if (lastException?.Message.Contains("133") == true)
                {
                    errorMessage += "Please try:\n1. Turn the printer off and on\n2. If that doesn't work, forget and re-pair the device in Bluetooth settings\n3. Restart your phone if the issue persists";
                }
                else
                {
                    errorMessage += "Please ensure the printer is turned on and in range, then try again.";
                }

                if (Application.Current!.Windows[0].Page != null)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlert("Connection Failed", errorMessage, "OK");
                }
                return false;
            }

            var service = await _connectedDevice.GetServiceAsync(_printerServiceUuid);
            if (service == null)
            {
                var services = await _connectedDevice.GetServicesAsync();
                service = services.FirstOrDefault();
            }

            if (service != null)
            {
                var characteristics = await service.GetCharacteristicsAsync();
                _writeCharacteristic = characteristics.FirstOrDefault(c =>
            c.CanWrite || c.Id == _writeCharacteristicUuid);
            }

            if (_connectedDevice != null && connected)
            {
                await NegotiateMtuAsync();
            }

            return _writeCharacteristic != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Negotiate MTU size with the connected device for optimal performance
    /// </summary>
    private Task NegotiateMtuAsync()
    {
        try
        {
            if (_connectedDevice == null || _writeCharacteristic == null)
                return Task.CompletedTask;
            _useWriteWithoutResponse = _writeCharacteristic.CanWrite &&
            (_writeCharacteristic.Properties.HasFlag(CharacteristicPropertyType.WriteWithoutResponse));
#if ANDROID
            try
            {
                _negotiatedMtu = 480; // 512 minus overhead for safety
            }
            catch
            {
                _negotiatedMtu = 180; // Fallback
            }
#endif
        }
        catch
        {
            _negotiatedMtu = DefaultChunkSize;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Write data to characteristic in optimized chunks
    /// </summary>
    private async Task<bool> WriteInChunksAsync(byte[] data, int delayBetweenChunks = 10)
    {
        if (_writeCharacteristic == null)
        {
            return false;
        }

        int offset = 0;
        try
        {
            int totalChunks = (data.Length + _negotiatedMtu - 1) / _negotiatedMtu;
            var startTime = DateTime.Now;

            byte[] chunkBuffer = new byte[_negotiatedMtu];
            int chunksWritten = 0;

            while (offset < data.Length)
            {
                int chunkSize = Math.Min(_negotiatedMtu, data.Length - offset);

                byte[] chunk = chunkSize == _negotiatedMtu ? chunkBuffer : new byte[chunkSize];
                Buffer.BlockCopy(data, offset, chunk, 0, chunkSize);

                if (_useWriteWithoutResponse && _writeCharacteristic.CanWrite)
                {
                    await _writeCharacteristic.WriteAsync(chunk);
                }
                else
                {
                    await _writeCharacteristic.WriteAsync(chunk);
                }

                offset += chunkSize;
                chunksWritten++;

                if (offset < data.Length && chunksWritten % 5 == 0)
                {
                    await Task.Delay(delayBetweenChunks);
                }
            }

            var elapsed = (DateTime.Now - startTime).TotalSeconds;
            var throughput = data.Length / 1024.0 / elapsed;

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }


    /// <summary>
    /// Print formatted text with custom font size and alignment
    /// Optimized for English text receipts
    /// </summary>
    public async Task<bool> PrintFormattedTextAsync(List<string> lines, int fontSize = 12, bool centerAlign = true, bool isBody = true)
    {
        if (_connectedDevice == null || _writeCharacteristic == null)
            return false;

        try
        {
            // Initialize printer
            await _writeCharacteristic.WriteAsync(EscPosCommands.Initialize());
            await Task.Delay(50);

            // Set alignment
            if (centerAlign)
            {
                await _writeCharacteristic.WriteAsync(EscPosCommands.CenterAlign());
            }
            else
            {
                await _writeCharacteristic.WriteAsync(EscPosCommands.LeftAlign());
            }
            await Task.Delay(20);

            // Set font size based on parameter
            byte[] fontSizeCommand = fontSize switch
            {
                <= 12 => [0x1B, 0x21, 0x00], // Normal
                <= 16 => [0x1B, 0x21, 0x10], // Double height
                <= 24 => [0x1B, 0x21, 0x20], // Double width
                _ => [0x1B, 0x21, 0x30]      // Double width and height
            };
            await _writeCharacteristic.WriteAsync(fontSizeCommand);
            await Task.Delay(20);

            // Print each line
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var lineBytes = System.Text.Encoding.UTF8.GetBytes(line + "\n");
                    bool writeSuccess = await WriteInChunksAsync(lineBytes, delayBetweenChunks: 5);
                    if (!writeSuccess)
                    {
                        return false;
                    }
                    await Task.Delay(10);
                }
            }

            if (isBody)
            {
                await Task.Delay(50);
                await _writeCharacteristic.WriteAsync(EscPosCommands.FeedLines(3));
                await Task.Delay(50);
                await _writeCharacteristic.WriteAsync(EscPosCommands.FullCut());
                await Task.Delay(50);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connectedDevice != null)
        {
            await _adapter.DisconnectDeviceAsync(_connectedDevice);
            _connectedDevice = null;
            _writeCharacteristic = null;
        }
    }
}
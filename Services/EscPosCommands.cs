
namespace OMS.Services;

/// <summary>
/// ESC/POS printer command generator
/// </summary>
public class EscPosCommands
{
    // ESC/POS control characters
    private const byte ESC = 0x1B;
    private const byte GS = 0x1D;
    private const byte LF = 0x0A;

    /// <summary>
    /// Initialize printer and reset to default settings
    /// </summary>
    public static byte[] Initialize()
    {
        return new byte[] { ESC, 0x40 };
    }

    /// <summary>
    /// Set character code page to UTF-8 (Code page 65001)
    /// This is essential for printing Unicode characters like Bengali
    /// </summary>
    public static byte[] SetUTF8()
    {
        // ESC t n - Select character code table
        // Most modern ESC/POS printers support code page selection
        // We'll use multiple methods to ensure UTF-8 is enabled

        var commands = new List<byte>();

        // Method 1: Set character code page to UTF-8 (if supported)
        // ESC t 16 or ESC t 255 for UTF-8 on some printers
        commands.AddRange(new byte[] { ESC, 0x74, 16 });

        // Method 2: Enable international character set
        // ESC R n - Select an international character set
        commands.AddRange(new byte[] { ESC, 0x52, 0x0F }); // 15 for UTF-8/Unicode

        return commands.ToArray();
    }

    /// <summary>
    /// Set character code page (for compatibility)
    /// Common values: 0=CP437 (USA), 16=CP1252 (Western Europe), 255=UTF-8
    /// </summary>
    public static byte[] SetCodePage(byte codePage)
    {
        return new byte[] { ESC, 0x74, codePage };
    }

    /// <summary>
    /// Feed paper by specified number of lines
    /// </summary>
    public static byte[] FeedLines(int lines)
    {
        return new byte[] { ESC, 0x64, (byte)lines };
    }

    /// <summary>
    /// Perform full paper cut
    /// </summary>
    public static byte[] FullCut()
    {
        return new byte[] { GS, 0x56, 0x00 };
    }

    /// <summary>
    /// Perform partial paper cut
    /// </summary>
    public static byte[] PartialCut()
    {
        return new byte[] { GS, 0x56, 0x01 };
    }

    /// <summary>
    /// Set left alignment
    /// </summary>
    public static byte[] LeftAlign()
    {
        return new byte[] { ESC, 0x61, 0x00 };
    }

    /// <summary>
    /// Set center alignment
    /// </summary>
    public static byte[] CenterAlign()
    {
        return new byte[] { ESC, 0x61, 0x01 };
    }

    /// <summary>
    /// Set right alignment
    /// </summary>
    public static byte[] RightAlign()
    {
        return new byte[] { ESC, 0x61, 0x02 };
    }

    /// <summary>
    /// Print text followed by line feed
    /// </summary>
    public static byte[] PrintLine(string text)
    {
        var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
        var result = new byte[textBytes.Length + 1];
        Array.Copy(textBytes, result, textBytes.Length);
        result[textBytes.Length] = LF;
        return result;
    }

    /// <summary>
    /// Print text without line feed
    /// </summary>
    public static byte[] PrintText(string text)
    {
        return System.Text.Encoding.UTF8.GetBytes(text);
    }

    /// <summary>
    /// Set bold mode on/off
    /// </summary>
    public static byte[] SetBold(bool enabled)
    {
        return new byte[] { ESC, 0x45, (byte)(enabled ? 1 : 0) };
    }

    /// <summary>
    /// Set underline mode
    /// </summary>
    public static byte[] SetUnderline(bool enabled)
    {
        return new byte[] { ESC, 0x2D, (byte)(enabled ? 1 : 0) };
    }

    /// <summary>
    /// Set font size (1-8 for width, 1-8 for height)
    /// </summary>
    public static byte[] SetFontSize(int width, int height)
    {
        if (width < 1 || width > 8) width = 1;
        if (height < 1 || height > 8) height = 1;

        byte size = (byte)(((width - 1) << 4) | (height - 1));
        return new byte[] { GS, 0x21, size };
    }

    /// <summary>
    /// Print image data in raster bit image mode
    /// Converts image bytes to ESC/POS raster format
    /// </summary>
    public static byte[] PrintImage(byte[] imageData, int width, int height)
    {
        // This is a simplified implementation
        // For production, you would need proper image processing
        var commands = new List<byte>();

        // Set raster bit image mode
        // GS v 0 m xL xH yL yH d1...dk
        commands.Add(GS);
        commands.Add(0x76);
        commands.Add(0x30);
        commands.Add(0x00); // Normal mode

        // Width in bytes (xL, xH)
        int widthBytes = (width + 7) / 8;
        commands.Add((byte)(widthBytes & 0xFF));
        commands.Add((byte)((widthBytes >> 8) & 0xFF));

        // Height (yL, yH)
        commands.Add((byte)(height & 0xFF));
        commands.Add((byte)((height >> 8) & 0xFF));

        // Add image data
        commands.AddRange(imageData);

        return commands.ToArray();
    }

    /// <summary>
    /// Set line spacing
    /// </summary>
    public static byte[] SetLineSpacing(int spacing)
    {
        return new byte[] { ESC, 0x33, (byte)spacing };
    }

    /// <summary>
    /// Reset line spacing to default
    /// </summary>
    public static byte[] ResetLineSpacing()
    {
        return new byte[] { ESC, 0x32 };
    }
}

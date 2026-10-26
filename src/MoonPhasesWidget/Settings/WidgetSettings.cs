using System.IO;
using System.Text.Json;

namespace MoonPhasesWidget.Settings;

public class WidgetSettings
{
    /// <summary>Window position in device-independent pixels; null until the user first moves it.</summary>
    public double? Left { get; set; }

    public double? Top { get; set; }

    public bool AlwaysOnTop { get; set; }

    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MoonPhasesWidget",
        "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Loads saved settings, falling back to defaults if the file is missing or unreadable.</summary>
    public static WidgetSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return JsonSerializer.Deserialize<WidgetSettings>(File.ReadAllText(FilePath)) ?? new();
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
        }

        return new();
    }

    /// <summary>Saves settings. Failures are ignored: losing the window position is not worth interrupting the user.</summary>
    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
        }
    }
}

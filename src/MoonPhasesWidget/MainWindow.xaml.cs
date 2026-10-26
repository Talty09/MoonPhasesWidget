using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MoonPhasesWidget.Astronomy;
using MoonPhasesWidget.Settings;

namespace MoonPhasesWidget;

public partial class MainWindow : Window
{
    // The phase changes slowly; once a minute keeps the countdowns current.
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromMinutes(1);
    private const double ScreenMargin = 24;

    private readonly WidgetSettings _settings = WidgetSettings.Load();
    private readonly DispatcherTimer _timer = new() { Interval = RefreshInterval };

    // Development aid: "--date 2026-11-02" (local time) shows that moment instead of now.
    private readonly DateTime? _previewTime = ParsePreviewTime(Environment.GetCommandLineArgs());

    public MainWindow()
    {
        InitializeComponent();

        Topmost = _settings.AlwaysOnTop;
        AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;

        Loaded += (_, _) => PlaceWindow();
        Closing += (_, _) => SavePosition();
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();

        Refresh();
    }

    private void Refresh()
    {
        var info = MoonPhaseCalculator.Calculate(_previewTime ?? DateTime.UtcNow);

        Moon.IlluminatedFraction = info.IlluminatedFraction;
        Moon.IsWaxing = info.IsWaxing;
        MoonGlow.Opacity = 0.08 + 0.37 * info.IlluminatedFraction;

        PhaseNameText.Text = info.Phase.ToDisplayName();
        IlluminationText.Text = $"{info.IlluminatedFraction:P0} lit · day {Math.Floor(info.AgeDays) + 1:0}";

        // List whichever event comes next first.
        string fullMoon = $"Full moon {info.NextFullMoonUtc.ToLocalTime():ddd MMM d}";
        string newMoon = $"New moon {info.NextNewMoonUtc.ToLocalTime():ddd MMM d}";
        (NextEventText.Text, FollowingEventText.Text) = info.IsWaxing ? (fullMoon, newMoon) : (newMoon, fullMoon);

        Panel.ToolTip =
            $"Next full moon: {info.NextFullMoonUtc.ToLocalTime():f}\n" +
            $"Next new moon: {info.NextNewMoonUtc.ToLocalTime():f}";
    }

    /// <summary>
    /// Restores the saved position, or uses the top-right corner of the work area when there
    /// is none or the saved spot is no longer on any screen (e.g. a monitor was disconnected).
    /// </summary>
    private void PlaceWindow()
    {
        if (_settings.Left is double left && _settings.Top is double top && IsOnScreen(left, top))
        {
            Left = left;
            Top = top;
            return;
        }

        Rect workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - ScreenMargin;
        Top = workArea.Top + ScreenMargin;
    }

    private bool IsOnScreen(double left, double top)
    {
        var screen = new Rect(
            SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenTop,
            SystemParameters.VirtualScreenWidth,
            SystemParameters.VirtualScreenHeight);

        return screen.IntersectsWith(new Rect(left, top, ActualWidth, ActualHeight));
    }

    private static DateTime? ParsePreviewTime(string[] args)
    {
        int index = Array.IndexOf(args, "--date");
        if (index < 0 || index + 1 >= args.Length)
        {
            return null;
        }

        return DateTime.TryParse(args[index + 1], CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var time)
            ? time
            : null;
    }

    private void SavePosition()
    {
        _settings.Left = Left;
        _settings.Top = Top;
        _settings.Save();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
        SavePosition();
    }

    private void AlwaysOnTop_Click(object sender, RoutedEventArgs e)
    {
        Topmost = AlwaysOnTopMenuItem.IsChecked;
        _settings.AlwaysOnTop = Topmost;
        _settings.Save();
    }

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
}

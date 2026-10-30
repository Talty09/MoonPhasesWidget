using System.Windows;

namespace MoonPhasesWidget;

public partial class App : Application
{
    // Also named by AppMutex in installer/MoonPhasesWidget.iss, so setup can tell when the widget is running.
    internal const string SingleInstanceMutexName = "MoonPhasesWidget.SingleInstance";

    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool createdNew);
        if (!createdNew)
        {
            // Another copy is already showing the widget (for example, started by both a shortcut and autostart).
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;
            Shutdown();
            return;
        }

        base.OnStartup(e);
        MainWindow = new MainWindow();
        MainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}

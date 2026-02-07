using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace Frontend;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things will break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBadPlace");
            Directory.CreateDirectory(logDir);
            string debugPath = Path.Combine(logDir, "debug.log");
            File.AppendAllText(debugPath, $"[CRITICAL ERROR] {DateTime.Now}: {ex}{Environment.NewLine}");
            
            string logPath = Path.Combine(logDir, "crash.log");
            File.WriteAllText(logPath, ex.ToString());
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}

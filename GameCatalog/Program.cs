using System;
using System.IO;
using Avalonia;
using Serilog;

namespace GameCatalog;

sealed class Program
{
    // ШІ: Claude Opus 5.5 (Medium) – відновлено шаблонну обгортку Avalonia (namespace, клас Program, [STAThread],
    //     BuildAvaloniaApp) і два коментарі до неї; ланцюжок LoggerConfiguration дано готовим.
    // Змінено: метод Main (журнал Serilog, try/catch/finally) написала студентка.
    // Точка входу: налаштування журналу Serilog і запуск застосунку.
    [STAThread]
    public static void Main(string[] args)
    {
    string logPath = Path.Combine(AppContext.BaseDirectory, "games.log");

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(logPath)
        .CreateLogger();

    try
    {
        Log.Information("Застосунок запущено");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Критична помилка під час роботи застосунку");
        throw;
    }
    finally
    {
        Log.Information("Застосунок закрито");
        Log.CloseAndFlush();
    }
}

    // Налаштування Avalonia (шаблон проєкту; використовується також дизайнером Rider)
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

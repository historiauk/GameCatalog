// ШІ: Claude Opus 5.5 (Medium) – ViewModel головного вікна (навігація, дашборд, пошук і фільтри, JSON, фоновий імпорт, паралельна статистика).
// Команди CRUD для ігор і копій та RunSafely – у файлі MainViewModel.Crud.cs (код студентки).
// Змінено: без змін
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameCatalog.Models;
using GameCatalog.Services;
using Serilog;

namespace GameCatalog.ViewModels;

// Логіка головного вікна: які дані показують таблиці і форми, що роблять кнопки.
// [ObservableProperty] – вікно саме дізнається, що значення змінилось, і перемальовується.
// [RelayCommand] – метод стає командою для кнопки: Command="{Binding AddGameCommand}".
public partial class MainViewModel : ViewModelBase
{
    private UserCollection _collection = new UserCollection();
    private readonly JsonStorage _storage = new JsonStorage();
    private readonly StatisticsService _statisticsService = new StatisticsService();
    private readonly CatalogFilter _filter = new CatalogFilter();
    private readonly DashboardService _dashboardService = new DashboardService();
    private readonly ImportService _importService = new ImportService();
    private readonly CatalogGenerator _generator = new CatalogGenerator();
    private readonly XmlExporter _xmlExporter = new XmlExporter();
    private CancellationTokenSource? _cts;   // «пульт» для скасування фонової операції
    private bool _isRefreshing;   // поки оновлюємо списки фільтрів – не фільтруємо зайвий раз

    // Неонові кольори для діаграм
    private static readonly IBrush[] NeonBrushes =
    {
        new SolidColorBrush(Color.Parse("#00F5FF")),
        new SolidColorBrush(Color.Parse("#FF2BD6")),
        new SolidColorBrush(Color.Parse("#A3FF12")),
        new SolidColorBrush(Color.Parse("#8B5CF6")),
        new SolidColorBrush(Color.Parse("#FFB800"))
    };

    // ---------- бокове меню ----------
    public string[] NavItems { get; } = { "Огляд", "Каталог ігор", "Моя колекція", "Імпорт і статистика" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOverviewPage), nameof(IsCatalogPage), nameof(IsCollectionPage), nameof(IsImportPage))]
    public partial int PageIndex { get; set; }

    public bool IsOverviewPage { get { return PageIndex == 0; } }
    public bool IsCatalogPage { get { return PageIndex == 1; } }
    public bool IsCollectionPage { get { return PageIndex == 2; } }
    public bool IsImportPage { get { return PageIndex == 3; } }

    // ---------- фоновий імпорт ----------
    [ObservableProperty] public partial string ImportPath { get; set; } = "";
    [ObservableProperty] public partial bool SlowMode { get; set; } = true;
    [ObservableProperty] public partial double ProgressValue { get; set; }
    [ObservableProperty] public partial string ProgressText { get; set; } = "";
    [ObservableProperty] public partial string ImportResultText { get; set; } = "";
    public ObservableCollection<string> OperationLog { get; } = new ObservableCollection<string>();

    // ---------- паралельна статистика ----------
    [ObservableProperty] public partial decimal? TaskCount { get; set; } = Environment.ProcessorCount;
    [ObservableProperty] public partial ObservableCollection<PlatformStat> Statistics { get; set; } = new ObservableCollection<PlatformStat>();
    [ObservableProperty] public partial string BenchmarkText { get; set; } = "";

    // Поки йде фонова операція – кнопки запуску вимкнені, а «Скасувати» увімкнена
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }
    public bool IsNotBusy { get { return !IsBusy; } }

    // ---------- дашборд «Огляд» ----------
    [ObservableProperty] public partial string TotalValueText { get; set; } = "";
    [ObservableProperty] public partial string CompletedPercentText { get; set; } = "";
    [ObservableProperty] public partial int CompletedCount { get; set; }
    [ObservableProperty] public partial double CompletedSweep { get; set; }
    [ObservableProperty] public partial List<ChartBar> PlatformBars { get; set; } = new List<ChartBar>();
    [ObservableProperty] public partial List<ChartBar> StatusBars { get; set; } = new List<ChartBar>();
    [ObservableProperty] public partial List<ChartBar> GenreBars { get; set; } = new List<ChartBar>();

    // Варіанти для випадаючих списків
    // (списки беруться з класу CatalogOptions – усі варіанти зберігаються в одному місці)
    public string[] PlatformList { get; } = CatalogOptions.Platforms;
    public string[] StatusList { get; } = CatalogOptions.Statuses;
    public string[] CopyTypeList { get; } = CatalogOptions.CopyTypes;

    // ---------- пошук і фільтри ----------
    [ObservableProperty] public partial string SearchText { get; set; } = "";
    [ObservableProperty] public partial List<string> GenreOptions { get; set; } = new List<string>();
    public List<string> PlatformOptions { get; }
    public List<string> StatusOptions { get; }
    [ObservableProperty] public partial string? SelectedGenre { get; set; } = CatalogFilter.AllOption;
    [ObservableProperty] public partial string? SelectedPlatform { get; set; } = CatalogFilter.AllOption;
    [ObservableProperty] public partial string? SelectedStatus { get; set; } = CatalogFilter.AllOption;

    // Загальна кількість (для карток «Огляду» – без урахування фільтрів)
    [ObservableProperty] public partial int TotalGames { get; set; }
    [ObservableProperty] public partial int TotalItems { get; set; }

    // ---------- дані для таблиць (уже відфільтровані) ----------
    [ObservableProperty] public partial ObservableCollection<Game> Games { get; set; } = new ObservableCollection<Game>();
    [ObservableProperty] public partial ObservableCollection<CollectionItem> Items { get; set; } = new ObservableCollection<CollectionItem>();
    [ObservableProperty] public partial List<string> GameTitles { get; set; } = new List<string>();

    // ---------- файл і повідомлення ----------
    [ObservableProperty] public partial string FilePath { get; set; } = "";
    [ObservableProperty] public partial string StatusMessage { get; set; } = "";
    [ObservableProperty] public partial string ErrorMessage { get; set; } = "";

    // ---------- форма гри ----------
    [ObservableProperty] public partial Game? SelectedGame { get; set; }
    [ObservableProperty] public partial string GameTitle { get; set; } = "";
    [ObservableProperty] public partial string GameGenre { get; set; } = "";
    [ObservableProperty] public partial decimal? GameYear { get; set; } = 2020;
    [ObservableProperty] public partial decimal? GameRating { get; set; } = 5;

    // ---------- форма копії ----------
    [ObservableProperty] public partial CollectionItem? SelectedItem { get; set; }
    [ObservableProperty] public partial string? ItemGameTitle { get; set; } = "";
    [ObservableProperty] public partial string? ItemPlatform { get; set; } = CatalogOptions.Platforms[0];
    [ObservableProperty, NotifyPropertyChangedFor(nameof(IsPhysical))]
    public partial string? ItemCopyType { get; set; } = "Цифрова";
    [ObservableProperty] public partial decimal? ItemPrice { get; set; } = 0;
    [ObservableProperty] public partial string? ItemStatus { get; set; } = CatalogOptions.Statuses[0];
    [ObservableProperty] public partial bool ItemIsSealed { get; set; }

    // Галочку «Запакована» показуємо лише для фізичної копії
    public bool IsPhysical
    {
        get { return ItemCopyType == "Фізична"; }
    }

    public MainViewModel()
    {
        PlatformOptions = MakeOptions(PlatformList);
        StatusOptions = MakeOptions(StatusList);
        FilePath = Path.Combine(AppContext.BaseDirectory, "Data", "games.json");
        ImportPath = Path.Combine(AppContext.BaseDirectory, "Data", "catalog_10000.json");
        LoadFromFile();
    }

    // =========================== файл JSON ===========================

    private void LoadFromFile()
    {
        ErrorMessage = "";
        try
        {
            _collection = _storage.Load(FilePath);
            RefreshTables();
            StatusMessage = "Завантажено ігор: " + _collection.Games.Count + ", копій: " + _collection.Items.Count;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не вдалося завантажити файл. " + ex.Message;
            Log.Error(ex, "Не вдалося завантажити {Path}", FilePath);
        }
    }

    [RelayCommand]
    private void Load()
    {
        LoadFromFile();
    }

    [RelayCommand]
    private void Save()
    {
        RunSafely(() =>
        {
            _storage.Save(_collection, FilePath);
            StatusMessage = "Збережено у файл " + FilePath;
        });
    }

    // Оновити таблиці, фільтри і дашборд після будь-якої зміни даних
    private void RefreshTables()
    {
        TotalGames = _collection.Games.Count;
        TotalItems = _collection.Items.Count;
        UpdateGenreOptions();

        List<string> titles = new List<string>();
        foreach (Game game in _collection.Games)
        {
            titles.Add(game.Title);
        }
        titles.Sort();
        GameTitles = titles;

        ApplyFilters();
        UpdateDashboard();
    }

    // =========================== пошук і фільтри ===========================

    // Коли змінюється пошук або будь-який фільтр – одразу перефільтровуємо таблиці
    partial void OnSearchTextChanged(string value) { ApplyFilters(); }
    partial void OnSelectedGenreChanged(string? value) { ApplyFilters(); }
    partial void OnSelectedPlatformChanged(string? value) { ApplyFilters(); }
    partial void OnSelectedStatusChanged(string? value) { ApplyFilters(); }

    // Показати в таблицях лише те, що підходить під пошук і фільтри
    private void ApplyFilters()
    {
        if (_isRefreshing)
        {
            return;
        }
        Games = new ObservableCollection<Game>(_filter.FilterGames(_collection.Games, SearchText, SelectedGenre));
        Items = new ObservableCollection<CollectionItem>(_filter.FilterItems(_collection.Items, SearchText, SelectedPlatform, SelectedStatus));
    }

    // Список жанрів для фільтра: «Усі» + усі жанри, які є в каталозі
    private void UpdateGenreOptions()
    {
        _isRefreshing = true;
        string? previous = SelectedGenre;

        List<string> genres = new List<string>();
        foreach (Game game in _collection.Games)
        {
            if (!string.IsNullOrWhiteSpace(game.Genre) && !genres.Contains(game.Genre))
            {
                genres.Add(game.Genre);
            }
        }
        genres.Sort();
        genres.Insert(0, CatalogFilter.AllOption);
        GenreOptions = genres;

        // якщо обраний жанр ще існує – залишаємо його, інакше «Усі»
        if (previous != null && genres.Contains(previous))
        {
            SelectedGenre = previous;
        }
        else
        {
            SelectedGenre = CatalogFilter.AllOption;
        }
        _isRefreshing = false;
    }

    // «Усі» + варіанти зі списку
    private List<string> MakeOptions(string[] values)
    {
        List<string> options = new List<string>();
        options.Add(CatalogFilter.AllOption);
        options.AddRange(values);
        return options;
    }

    // =========================== дашборд ===========================

    // Перерахувати картки і діаграми сторінки «Огляд»
    private void UpdateDashboard()
    {
        // 1) картки і кільцева діаграма – з твоєї статистики за платформами
        CollectionStatistics stats = _statisticsService.Calculate(_collection.Items);
        decimal totalValue = 0;
        int completed = 0;
        int maxPlatform = 1;
        foreach (PlatformStat stat in stats.Platforms)
        {
            totalValue += stat.TotalValue;
            completed += stat.CompletedCount;
            maxPlatform = Math.Max(maxPlatform, stat.ItemCount);
        }

        double percent = 0;
        if (_collection.Items.Count > 0)
        {
            percent = completed * 100.0 / _collection.Items.Count;
        }
        TotalValueText = totalValue.ToString("N0") + " грн";
        CompletedCount = completed;
        CompletedPercentText = percent.ToString("0") + "%";
        CompletedSweep = Math.Max(0.1, percent * 3.6);   // 100% = повне коло 360°

        // 2) стовпчики «копії за платформами» (висота до 150 px)
        List<ChartBar> platformBars = new List<ChartBar>();
        int colorIndex = 0;
        foreach (PlatformStat stat in stats.Platforms)
        {
            platformBars.Add(new ChartBar
            {
                Label = stat.Platform,
                ValueText = stat.ItemCount.ToString(),
                Size = 150.0 * stat.ItemCount / maxPlatform,
                Brush = NeonBrushes[colorIndex % NeonBrushes.Length]
            });
            colorIndex++;
        }
        PlatformBars = platformBars;

        // 3) смужки «статуси проходження» – підрахунок у DashboardService
        Dictionary<string, int> statusCounts = _dashboardService.CountByStatus(_collection.Items);
        StatusBars = MakeHorizontalBars(new List<KeyValuePair<string, int>>(statusCounts));

        // 4) смужки «топ-5 жанрів» – підрахунок у DashboardService, тут лише сортування і перші 5
        List<KeyValuePair<string, int>> genreCounts =
            new List<KeyValuePair<string, int>>(_dashboardService.CountByGenre(_collection.Games));
        genreCounts.Sort((a, b) => b.Value.CompareTo(a.Value));   // від більшого до меншого
        if (genreCounts.Count > 5)
        {
            genreCounts = genreCounts.GetRange(0, 5);
        }
        GenreBars = MakeHorizontalBars(genreCounts);
    }

    // Перетворити пари "назва – кількість" на горизонтальні смужки (ширина до 220 px)
    private List<ChartBar> MakeHorizontalBars(List<KeyValuePair<string, int>> counts)
    {
        int max = 1;
        foreach (KeyValuePair<string, int> pair in counts)
        {
            max = Math.Max(max, pair.Value);
        }

        List<ChartBar> bars = new List<ChartBar>();
        int colorIndex = 0;
        foreach (KeyValuePair<string, int> pair in counts)
        {
            bars.Add(new ChartBar
            {
                Label = pair.Key,
                ValueText = pair.Value.ToString(),
                Size = Math.Max(4, 220.0 * pair.Value / max),
                Brush = NeonBrushes[colorIndex % NeonBrushes.Length]
            });
            colorIndex++;
        }
        return bars;
    }

    // =========================== генератор і фоновий імпорт ===========================

    // Згенерувати файл catalog_10000.json (у фоновому потоці, щоб вікно не завмирало)
    [RelayCommand]
    private async Task GenerateCatalogAsync()
    {
        StartOperation();
        try
        {
            string path = ImportPath;
            await Task.Run(() =>
            {
                UserCollection data = _generator.Generate(10000);
                _storage.Save(data, path);
            });
            ProgressValue = 100;
            ProgressText = "100%";
            ImportResultText = "Згенеровано 10 000 записів у файл " + path + ". Тепер натисни «Імпортувати».";
            AddLog("Згенеровано каталог на 10 000 записів");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Помилка генерації: " + ex.Message;
            Log.Error(ex, "Помилка генерації каталогу");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Фоновий імпорт з перевіркою дублікатів, прогресом і можливістю скасування
    [RelayCommand]
    private async Task ImportAsync()
    {
        StartOperation();

        // Progress<int>, створений у потоці інтерфейсу, сам передає відсотки назад у цей потік
        Progress<int> progress = new Progress<int>(percent =>
        {
            ProgressValue = percent;
            ProgressText = percent + "%";
        });

        // КОПІЇ назв і ключів – фоновий потік працює з ними, а не з даними, які показує вікно
        HashSet<string> knownTitles = new HashSet<string>();
        foreach (Game game in _collection.Games)
        {
            knownTitles.Add(ImportService.MakeKey(game.Title));
        }
        HashSet<string> knownItemKeys = new HashSet<string>();
        foreach (CollectionItem item in _collection.Items)
        {
            knownItemKeys.Add(ImportService.MakeKey(item.GameTitle) + "|" + ImportService.MakeKey(item.Platform));
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            // await – чекаємо фонову задачу, НЕ блокуючи вікно
            ImportResult result = await _importService.ImportAsync(
                ImportPath, knownTitles, knownItemKeys, progress, AddLogFromBackground, SlowMode, _cts!.Token);

            // після await ми знову в потоці інтерфейсу – тут можна змінювати дані, які показує вікно
            _collection.Games.AddRange(result.NewGames);
            _collection.Items.AddRange(result.NewItems);
            RefreshTables();
            StatusMessage = "У каталозі ігор: " + _collection.Games.Count + ", копій: " + _collection.Items.Count;

            ImportResultText = "Імпорт завершено за " + stopwatch.ElapsedMilliseconds + " мс.\n"
                + "Додано ігор: " + result.NewGames.Count + ", копій: " + result.NewItems.Count + "\n"
                + "Пропущено дублікатів ігор: " + result.DuplicateGames + ", копій: " + result.DuplicateItems
                + ", копій без гри: " + result.SkippedItems;
            AddLog("Імпорт завершено, додано ігор: " + result.NewGames.Count);
        }
        catch (OperationCanceledException)
        {
            ImportResultText = "Імпорт скасовано. Каталог не змінено.";
            AddLog("Імпорт скасовано користувачем");
            Log.Warning("Імпорт скасовано користувачем");
        }
        catch (FileNotFoundException ex)
        {
            ErrorMessage = ex.Message;
            Log.Error("Файл для імпорту не знайдено: {Path}", ImportPath);
        }
        catch (InvalidDataException ex)
        {
            ErrorMessage = ex.Message;   // пошкоджений файл – уже записано в журнал у JsonStorage
        }
        catch (Exception ex)
        {
            ErrorMessage = "Помилка імпорту: " + ex.Message;
            Log.Error(ex, "Помилка імпорту");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // =========================== паралельна статистика ===========================

    // Скільки задач обрано (від 1 до 64)
    private int GetTaskCount()
    {
        int count = (int)(TaskCount ?? 1);
        return Math.Clamp(count, 1, 64);
    }

    // Розрахувати статистику в кількох задачах і показати в таблиці
    [RelayCommand]
    private async Task CalculateStatisticsAsync()
    {
        StartOperation();
        Progress<int> progress = new Progress<int>(percent =>
        {
            ProgressValue = percent;
            ProgressText = percent + "%";
        });

        // знімок (копія) списку – фонові задачі читають його, а не список, який показує вікно
        List<CollectionItem> items = new List<CollectionItem>(_collection.Items);
        int taskCount = GetTaskCount();

        try
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            CollectionStatistics stats = await _statisticsService.CalculateParallelAsync(
                items, taskCount, progress, AddLogFromBackground, _cts!.Token);
            stopwatch.Stop();

            Statistics = new ObservableCollection<PlatformStat>(stats.Platforms);
            ImportResultText = "Статистику " + items.Count + " копій пораховано в " + taskCount
                               + " задачах за " + stopwatch.Elapsed.TotalMilliseconds.ToString("F2") + " мс";
            Log.Information("Статистика: {Count} копій, {Tasks} задач, {Ms} мс", items.Count, taskCount, stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (OperationCanceledException)
        {
            ImportResultText = "Розрахунок статистики скасовано";
            Log.Warning("Розрахунок статистики скасовано користувачем");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Помилка розрахунку: " + ex.Message;
            Log.Error(ex, "Помилка розрахунку статистики");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Порівняти однопотоковий (T1) і багатопотоковий (Tn) розрахунок.
    // Кожен варіант виконується 20 разів, береться середній час. S = T1 / Tn
    [RelayCommand]
    private async Task CompareAsync()
    {
        const int runs = 20;
        StartOperation();
        List<CollectionItem> items = new List<CollectionItem>(_collection.Items);
        int taskCount = GetTaskCount();
        CancellationToken token = _cts!.Token;

        try
        {
            // T1 – в одному потоці (у фоні, щоб вікно не завмирало)
            double t1 = await Task.Run(() =>
            {
                _statisticsService.Calculate(items);   // «розігрів» – перший запуск завжди повільніший
                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < runs; i++)
                {
                    token.ThrowIfCancellationRequested();
                    _statisticsService.Calculate(items);
                }
                return stopwatch.Elapsed.TotalMilliseconds / runs;
            }, token);
            ProgressValue = 50;
            ProgressText = "50%";

            // Tn – у taskCount задачах
            double tn = await Task.Run(async () =>
            {
                await _statisticsService.CalculateParallelAsync(items, taskCount, null, null, token);   // «розігрів»
                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int i = 0; i < runs; i++)
                {
                    await _statisticsService.CalculateParallelAsync(items, taskCount, null, null, token);
                }
                return stopwatch.Elapsed.TotalMilliseconds / runs;
            }, token);
            ProgressValue = 100;
            ProgressText = "100%";

            double speedup = tn > 0 ? t1 / tn : 0;
            BenchmarkText = "Копій: " + items.Count + ", повторів: " + runs + "\n"
                            + "T1 (1 потік) = " + t1.ToString("F3") + " мс\n"
                            + "T" + taskCount + " (" + taskCount + " задач) = " + tn.ToString("F3") + " мс\n"
                            + "S = T1 / T" + taskCount + " = " + speedup.ToString("F2");
            AddLog("Порівняння: T1 = " + t1.ToString("F3") + " мс, T" + taskCount + " = " + tn.ToString("F3") + " мс, S = " + speedup.ToString("F2"));
            Log.Information("Порівняння: копій {Count}, T1={T1:F3} мс, Tn={Tn:F3} мс (n={N}), S={S:F2}",
                items.Count, t1, tn, taskCount, speedup);
        }
        catch (OperationCanceledException)
        {
            BenchmarkText = "Порівняння скасовано";
        }
        catch (Exception ex)
        {
            ErrorMessage = "Помилка порівняння: " + ex.Message;
            Log.Error(ex, "Помилка порівняння");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Експорт статистики у Data/statistics.xml (через XmlExporter)
    [RelayCommand]
    private void ExportXml()
    {
        RunSafely(() =>
        {
            CollectionStatistics stats = _statisticsService.Calculate(new List<CollectionItem>(_collection.Items));
            Statistics = new ObservableCollection<PlatformStat>(stats.Platforms);
            string path = Path.Combine(AppContext.BaseDirectory, "Data", "statistics.xml");
            _xmlExporter.Export(stats, path);
            ImportResultText = "Статистику експортовано у файл " + path;
            Log.Information("Статистику експортовано в XML: {Path}", path);
        });
    }

    // Кнопка «Скасувати»: подати сигнал фоновій задачі
    [RelayCommand]
    private void Cancel()
    {
        if (_cts != null)
        {
            _cts.Cancel();
        }
    }

    // Підготовка до довгої операції: новий «пульт» скасування, обнулити прогрес
    private void StartOperation()
    {
        _cts = new CancellationTokenSource();
        IsBusy = true;
        ProgressValue = 0;
        ProgressText = "0%";
        ErrorMessage = "";
        ImportResultText = "";
    }

    // Додати рядок у журнал на екрані (лише з потоку інтерфейсу)
    private void AddLog(string message)
    {
        OperationLog.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "   " + message);
        if (OperationLog.Count > 100)
        {
            OperationLog.RemoveAt(OperationLog.Count - 1);
        }
    }

    // Викликається з ФОНОВОГО потоку. Змінювати вікно з фонового потоку не можна,
    // тому передаємо дію в потік інтерфейсу через Dispatcher.UIThread.Post
    private void AddLogFromBackground(string message)
    {
        Dispatcher.UIThread.Post(() => AddLog(message));
    }
}

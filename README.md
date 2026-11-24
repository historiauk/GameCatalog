# Каталог відеоігор з колекцією користувача

Курсова робота з дисципліни «Об’єктно-орієнтоване програмування».
ВСП «Одеський технічний фаховий коледж ОНТУ», група 3РП-11.

Настільний застосунок для обліку відеоігор: каталог ігор і колекція користувача
(цифрові, фізичні копії та ігри за підпискою) з пошуком, фільтрами, статистикою,
імпортом/експортом JSON і XML та фоновою обробкою великих каталогів.

## Технології

- C# / .NET 10
- Avalonia UI 12, CommunityToolkit.Mvvm (архітектура MVVM)
- System.Text.Json (JSON), XmlSerializer (XML)
- Serilog (журнал `games.log`)

## Можливості

- CRUD ігор і копій колекції з перевіркою даних (назва обов’язкова, рік від 1970, ціна ≥ 0, рейтинг 1–10, без дублікатів гри на платформі)
- пошук за назвою, фільтри за жанром, платформою і статусом, сортування за рейтингом
- читання і запис каталогу в JSON, експорт статистики за платформами в XML
- генерація тестового каталогу на 10 000 записів і фоновий імпорт з перевіркою дублікатів, індикатором прогресу і скасуванням
- паралельний розрахунок статистики в кількох задачах (`Task.Run` + `lock`) і порівняння з однопотоковим (S = T1 / Tn)
- обробка винятків (пошкоджений файл, дублікат) і журнал подій у `games.log`

## Структура проєкту

```
GameCatalog/
├─ Models/      доменні класи: Game, IRateable, CollectionItem, DigitalCopy, PhysicalCopy,
│               SubscriptionCopy, UserCollection, PlatformStat, CollectionStatistics, DuplicateGameException
├─ Services/    JsonStorage, XmlExporter, CatalogFilter, StatisticsService, ImportService, CatalogGenerator
├─ ViewModels/  MainViewModel, ChartBar
├─ Views/       MainWindow.axaml
└─ Data/        games.json (52 гри), broken.json (пошкоджений файл для тестування)
```

## Запуск

Потрібен .NET 10 SDK.

```
dotnet run --project GameCatalog
```

Зібрати один виконуваний файл для Windows:

```
dotnet publish GameCatalog -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Готова програма – у папці `publish` (`GameCatalog.exe` разом із папкою `Data`).

## Використання ШІ

Під час роботи використовувався ШІ-асистент Claude Opus 5.5 (Anthropic). Фрагменти коду, отримані від ШІ,
позначені коментарем `// ШІ: ...`; повний журнал – у файлі `AI_LOG.md`.

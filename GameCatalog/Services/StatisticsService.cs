using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameCatalog.Models;

namespace GameCatalog.Services;

public class StatisticsService
{
    // Рахує статистику для частини списку копій.
    private Dictionary<string, PlatformStat> CalculatePart(List<CollectionItem> items, int start, int end, CancellationToken token)
    {
        Dictionary<string, PlatformStat> stats = new Dictionary<string, PlatformStat>();

        for (int i = start; i < end; i++)
        {
            // Перевіряємо, чи не потрібно скасувати обчислення.
            if (i % 500 == 0)
            {
                token.ThrowIfCancellationRequested();
            }

            CollectionItem item = items[i];

            // Створюємо запис для нової платформи.
            if (!stats.ContainsKey(item.Platform))
            {
                stats[item.Platform] = new PlatformStat { Platform = item.Platform };
            }

            // Оновлюємо статистику поточної платформи.
            PlatformStat stat = stats[item.Platform];

            stat.ItemCount++;
            stat.TotalPrice += item.Price;
            stat.TotalValue += item.GetValue();

            // Рахуємо кількість пройдених ігор.
            if (item.Status == CatalogOptions.CompletedStatus)
            {
                stat.CompletedCount++;
            }
        }

        return stats;
    }

    // Формує готовий результат статистики з отриманих даних.
    private CollectionStatistics BuildResult(Dictionary<string, PlatformStat> stats, int totalItems)
    {
        CollectionStatistics result = new CollectionStatistics();

        // Записуємо дату створення та загальну кількість копій.
        result.CreatedAt = DateTime.Now;
        result.TotalItems = totalItems;

        // Додаємо статистику всіх платформ.
        foreach (PlatformStat stat in stats.Values)
        {
            result.Platforms.Add(stat);
        }

        // Сортуємо платформи за кількістю копій від більшої до меншої.
        result.Platforms.Sort((a, b) => b.ItemCount.CompareTo(a.ItemCount));

        return result;
    }

    // Виконує однопотоковий розрахунок статистики всієї колекції.
    public CollectionStatistics Calculate(List<CollectionItem> items)
    {
        Dictionary<string, PlatformStat> stats = CalculatePart(items, 0, items.Count, CancellationToken.None);

        return BuildResult(stats, items.Count);
    }
        // Розподіляє розрахунок статистики між кількома потоками.
    public async Task<CollectionStatistics> CalculateParallelAsync(List<CollectionItem> items, int taskCount,
        IProgress<int>? progress, Action<string>? onPartDone, CancellationToken token)
    {
        // Перевіряємо кількість задач.
        if (taskCount < 1)
        {
            taskCount = 1;
        }

        // Створюємо спільний словник і блокування для безпечного доступу.
        Dictionary<string, PlatformStat> total = new Dictionary<string, PlatformStat>();
        object locker = new object();

        // Визначаємо розмір кожної частини списку.
        int partSize = (items.Count + taskCount - 1) / taskCount;
        int partsCount = partSize == 0 ? 0 : (items.Count + partSize - 1) / partSize;
        int finishedParts = 0;

        // Створюємо список задач для паралельного виконання.
        List<Task> tasks = new List<Task>();

        // Розподіляємо список на частини та запускаємо їх обробку.
        for (int part = 0; part < partsCount; part++)
        {
            int start = part * partSize;
            int end = Math.Min(start + partSize, items.Count);
            int partNumber = part + 1;

            tasks.Add(Task.Run(() =>
            {
                // Рахуємо статистику для поточної частини.
                Dictionary<string, PlatformStat> local = CalculatePart(items, start, end, token);

                // Безпечно об'єднуємо результати в спільному словнику.
                lock (locker)
                {
                    foreach (PlatformStat stat in local.Values)
                    {
                        if (!total.ContainsKey(stat.Platform))
                        {
                            total[stat.Platform] = new PlatformStat { Platform = stat.Platform };
                        }

                        total[stat.Platform].Add(stat);
                    }

                    finishedParts++;

                    // Повідомляємо про завершення чергової частини.
                    if (progress != null)
                    {
                        progress.Report(finishedParts * 100 / partsCount);
                    }
                }

                // Записуємо інформацію про виконану частину.
                if (onPartDone != null)
                {
                    onPartDone("Частину " + partNumber + " (записи " + start + "–" + (end - 1) +
                        ") пораховано в потоці №" + Environment.CurrentManagedThreadId);
                }

            }, token));
        }

        // Чекаємо завершення всіх запущених задач.
        await Task.WhenAll(tasks);

        // Формуємо остаточний результат статистики.
        return BuildResult(total, items.Count);
    }
}
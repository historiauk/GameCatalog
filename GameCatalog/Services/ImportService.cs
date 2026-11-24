using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameCatalog.Models;
using Serilog;

namespace GameCatalog.Services;

// Відповідає за імпорт даних до каталогу.
public class ImportService
{
    // Створюємо сховище для читання файлів.
    private readonly JsonStorage _storage = new JsonStorage();

    // Створює уніфікований ключ для порівняння тексту.
    public static string MakeKey(string text)
    {
        return text.Trim().ToLower();
    }
        // Виконує імпорт ігор та копій із перевіркою дублікатів.
    private ImportResult Import(string path, HashSet<string> knownTitles, HashSet<string> knownItemKeys,
        IProgress<int> progress, Action<string> onStatus, bool slowMode, CancellationToken token)
    {
        // Повідомляємо про початок імпорту та записуємо подію в журнал.
        onStatus("Фоновий імпорт почався в потоці №" + Environment.CurrentManagedThreadId);
        Log.Information("Фоновий імпорт {Path}", path);

        // Завантажуємо дані з файлу.
        UserCollection source = _storage.Load(path);

        // Створюємо результат імпорту та лічильники оброблених записів.
        ImportResult result = new ImportResult();
        int total = Math.Max(1, source.Games.Count + source.Items.Count);
        int done = 0;

        // Перевіряємо ігри та відкидаємо дублікати.
        foreach (var game in source.Games)
        {
            token.ThrowIfCancellationRequested();

            string key = MakeKey(game.Title);

            if (knownTitles.Contains(key))
            {
                result.DuplicateGames++;
            }
            else
            {
                knownTitles.Add(key);
                result.NewGames.Add(game);
            }

            done++;
            ReportProgress(done, total, progress, onStatus, slowMode);
        }

        // Перевіряємо копії та відкидаємо повторні записи.
        foreach (var item in source.Items)
        {
            token.ThrowIfCancellationRequested();

            if (!knownTitles.Contains(MakeKey(item.GameTitle)))
            {
                result.SkippedItems++;
            }
            else
            {
                string itemKey = MakeKey(item.GameTitle) + "|" + MakeKey(item.Platform);

                if (knownItemKeys.Contains(itemKey))
                {
                    result.DuplicateItems++;
                }
                else
                {
                    knownItemKeys.Add(itemKey);
                    result.NewItems.Add(item);
                }
            }

            done++;
            ReportProgress(done, total, progress, onStatus, slowMode);
        }

        // Повідомляємо про завершення та записуємо підсумок у журнал.
        progress.Report(100);

        Log.Information("Імпорт завершено: нових ігор {Games}, копій {Items}",
            result.NewGames.Count, result.NewItems.Count);

        return result;
    }
    // Запускає імпорт у фоновому потоці, щоб вікно не зависало.
    public Task<ImportResult> ImportAsync(string path, HashSet<string> knownTitles, HashSet<string> knownItemKeys,
        IProgress<int> progress, Action<string> onStatus, bool slowMode, CancellationToken token)
    {
        return Task.Run(() => Import(path, knownTitles, knownItemKeys, progress, onStatus, slowMode, token), token);
    }
        // Оновлює відсоток виконання та повідомляє про хід імпорту.
        private void ReportProgress(int done, int total, IProgress<int> progress, Action<string> onStatus, bool slowMode)
        {
            // Оновлюємо прогрес кожні 100 оброблених записів.
            if (done % 100 == 0)
            {
                progress.Report(done * 100 / total);
    
                // Робимо невелику затримку для демонстрації повільного режиму.
                if (slowMode)
                {
                    Thread.Sleep(5);
                }
            }
    
            // Повідомляємо про хід імпорту кожні 2000 записів.
            if (done % 2000 == 0)
            {
                onStatus("Оброблено " + done + " з " + total +
                    " записів (потік №" + Environment.CurrentManagedThreadId + ")");
            }
        }
}
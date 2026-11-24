using System.Collections.Generic;
using GameCatalog.Models;

namespace GameCatalog.Services;

// Відповідає за підрахунок статистики для сторінки огляду.
public class DashboardService
{
    // Підраховує кількість копій ігор за кожним статусом.
    public Dictionary<string, int> CountByStatus(List<CollectionItem> items)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();

        // Додаємо всі доступні статуси з початковим значенням 0.
        foreach (string status in CatalogOptions.Statuses)
        {
            counts[status] = 0;
        }

        // Збільшуємо лічильник для кожного знайденого статусу.
        foreach (var item in items)
        {
            if (counts.ContainsKey(item.Status))
            {
                counts[item.Status]++;
            }
        }

        return counts;
    }

    // Підраховує кількість ігор за жанрами.
    public Dictionary<string, int> CountByGenre(List<Game> games)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();

        foreach (var game in games)
        {
            // Отримуємо жанр гри.
            string genre = game.Genre;

            // Замінюємо порожній жанр на стандартну назву.
            if (string.IsNullOrWhiteSpace(genre))
            {
                genre = "Без жанру";
            }

            // Додаємо новий жанр до словника, якщо його ще немає.
            if (!counts.ContainsKey(genre))
            {
                counts[genre] = 0;
            }

            // Збільшуємо кількість ігор цього жанру.
            counts[genre]++;
        }

        return counts;
    }
}
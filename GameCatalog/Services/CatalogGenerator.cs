using System;
using GameCatalog.Models;

namespace GameCatalog.Services;

public class CatalogGenerator
{
    // Жанри ігор, які використовуються під час генерації.
    private static readonly string[] Genres =
    {
        "RPG", "Шутер", "Гонки", "Пригоди", "Стратегія",
        "Жахи", "Симулятор", "Платформер", "Екшн", "Головоломка"
    };

    // Перші слова для формування назв ігор.
    private static readonly string[] Words1 =
    {
        "Dark", "Lost", "Eternal", "Neon", "Silent",
        "Ancient", "Frozen", "Broken", "Golden", "Hidden"
    };

    // Другі слова для формування назв ігор.
    private static readonly string[] Words2 =
    {
        "Kingdom", "Legends", "Horizon", "World", "Empire",
        "Dreams", "Shadows", "Journey", "Chronicles", "Destiny"
    };

    // Створює каталог із заданою кількістю ігор та копій.
    public UserCollection Generate(int count)
    {
        Random random = new Random(42);
        UserCollection data = new UserCollection();

        for (int i = 0; i < count; i++)
        {
            string title;

            if (data.Games.Count > 10 && random.NextDouble() < 0.02)
            {
                title = data.Games[random.Next(data.Games.Count)].Title;
            }
            else
            {
                title = Words1[random.Next(Words1.Length)] + " " +
                    Words2[random.Next(Words2.Length)] + " " + (i + 1);
            }

            Game game = new Game();
            game.Title = title;
            game.Genre = Genres[random.Next(Genres.Length)];
            game.Year = random.Next(1970, 2027);
            game.Rating = random.Next(1, 11);

            data.Games.Add(game);

            int type = random.Next(3);
            CollectionItem item;

            if (type == 0)
            {
                item = new DigitalCopy();
            }
            else if (type == 1)
            {
                PhysicalCopy physical = new PhysicalCopy();
                physical.IsSealed = random.Next(2) == 0;
                item = physical;
            }
            else
            {
                item = new SubscriptionCopy();
            }

            item.GameTitle = title;
            item.Platform = CatalogOptions.Platforms[random.Next(CatalogOptions.Platforms.Length)];
            item.Status = CatalogOptions.Statuses[random.Next(CatalogOptions.Statuses.Length)];
            item.Price = Math.Round((decimal)(random.NextDouble() * 2500), 2);

            data.Items.Add(item);
        }

        return data;
    }
}
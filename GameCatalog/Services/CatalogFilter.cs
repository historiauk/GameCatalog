using System;
using System.Collections.Generic;
using GameCatalog.Models;

namespace GameCatalog.Services;

public class CatalogFilter
{
    public const string AllOption = "Усі";

    // Перевіряє, чи вибрано всі варіанти.
    private bool IsAll(string? value)
    {
        return string.IsNullOrEmpty(value) || value == AllOption;
    }

    // Перевіряє, чи містить текст пошуковий запит.
    private bool ContainsText(string text, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;

        return text.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    // Фільтрує ігри за назвою та жанром.
    public List<Game> FilterGames(List<Game> games, string search, string? genre)
    {
        List<Game> result = new List<Game>();

        foreach (var game in games)
        {
            if (!ContainsText(game.Title, search))
                continue;

            if (!IsAll(genre) && game.Genre != genre)
                continue;

            result.Add(game);
        }

        result.Sort((a, b) => b.Rating.CompareTo(a.Rating));

        return result;
    }
    // Фільтрує елементи колекції за назвою, платформою та статусом.
    public List<CollectionItem> FilterItems(List<CollectionItem> items, string search, string? platform, string? status)
    {
        List<CollectionItem> result = new List<CollectionItem>();

        foreach (var item in items)
        {
            if (!ContainsText(item.GameTitle, search))
                continue;

            if (!IsAll(platform) && item.Platform != platform)
                continue;

            if (!IsAll(status) && item.Status != status)
                continue;

            result.Add(item);
        }

        result.Sort((a, b) => string.Compare(a.GameTitle, b.GameTitle, StringComparison.CurrentCultureIgnoreCase));

        return result;
    }
}
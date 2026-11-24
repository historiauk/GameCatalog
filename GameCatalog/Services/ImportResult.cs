using System.Collections.Generic;
using GameCatalog.Models;

namespace GameCatalog.Services;

// Зберігає результати імпорту ігор та копій.
public class ImportResult
{
    // Список нових ігор для додавання до каталогу.
    public List<Game> NewGames { get; } = new List<Game>();

    // Список нових копій для додавання до колекції.
    public List<CollectionItem> NewItems { get; } = new List<CollectionItem>();

    // Кількість ігор, які вже були в каталозі.
    public int DuplicateGames { get; set; }

    // Кількість копій, які вже існували в колекції.
    public int DuplicateItems { get; set; }

    // Кількість записів, які не вдалося імпортувати.
    public int SkippedItems { get; set; }
}

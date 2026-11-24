using System;
using CommunityToolkit.Mvvm.Input;
using GameCatalog.Models;
using Serilog;

namespace GameCatalog.ViewModels;

// Команди для роботи з іграми.
public partial class MainViewModel
{
    // Безпечно виконує дію та обробляє можливі помилки.
    private void RunSafely(Action action)
    {
        ErrorMessage = "";

        try
        {
            action();
        }
        catch (DuplicateGameException ex)
        {
            ErrorMessage = ex.Message;
            Log.Warning("Дублікат: {Message}", ex.Message);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
            Log.Warning("Помилка введення: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Помилка: " + ex.Message;
            Log.Error(ex, "Неочікувана помилка");
        }
    }

    // Заповнює поля форми даними вибраної гри.
    partial void OnSelectedGameChanged(Game? value)
    {
        if (value == null)
        {
            return;
        }

        GameTitle = value.Title;
        GameGenre = value.Genre;
        GameYear = value.Year;
        GameRating = value.Rating;
    }

    // Створює об'єкт гри з даних, введених у форму.
    private Game ReadGameForm()
    {
        Game game = new Game();

        game.Title = GameTitle.Trim();
        game.Genre = GameGenre.Trim();
        game.Year = (int)(GameYear ?? 0);
        game.Rating = (int)(GameRating ?? 0);

        return game;
    }

    // Очищає форму для введення нової гри.
    [RelayCommand]
    private void NewGame()
    {
        SelectedGame = null;
        GameTitle = "";
        GameGenre = "";
        GameYear = 2020;
        GameRating = 5;
        ErrorMessage = "";
    }

    // Додає нову гру до каталогу.
    [RelayCommand]
    private void AddGame()
    {
        RunSafely(() =>
        {
            Game game = ReadGameForm();

            _collection.AddGame(game);

            RefreshTables();

            StatusMessage = "Гру «" + game.Title + "» додано";

            Log.Information("Додано гру {Title}", game.Title);
        });
    }

    // Зберігає зміни вибраної гри.
    [RelayCommand]
    private void SaveGame()
    {
        RunSafely(() =>
        {
            if (SelectedGame == null)
            {
                throw new ArgumentException("Спочатку оберіть гру в таблиці");
            }

            Game selected = SelectedGame;
            Game newGame = ReadGameForm();

            _collection.ReplaceGame(selected, newGame);

            RefreshTables();

            StatusMessage = "Гру «" + newGame.Title + "» змінено";

            Log.Information("Змінено гру {Title}", newGame.Title);
        });
    }

    // Видаляє вибрану гру та пов'язані з нею копії.
    [RelayCommand]
    private void DeleteGame()
    {
        RunSafely(() =>
        {
            if (SelectedGame == null)
            {
                throw new ArgumentException("Спочатку оберіть гру в таблиці");
            }

            Game selected = SelectedGame;
            string title = selected.Title;

            _collection.RemoveGame(selected);

            RefreshTables();
            NewGame();

            StatusMessage = "Гру «" + title + "» і її копії видалено";

            Log.Information("Видалено гру {Title}", title);
        });
    }
        // Заповнює поля форми даними вибраної копії.
    partial void OnSelectedItemChanged(CollectionItem? value)
    {
        if (value == null)
        {
            return;
        }

        ItemGameTitle = value.GameTitle;
        ItemPlatform = value.Platform;
        ItemPrice = value.Price;
        ItemStatus = value.Status;
        ItemCopyType = value.TypeName;

        ItemIsSealed = value is PhysicalCopy physical && physical.IsSealed;
    }

    // Створює копію потрібного типу з даних форми.
    private CollectionItem ReadItemForm()
    {
        CollectionItem item;

        if (ItemCopyType == "Фізична")
        {
            PhysicalCopy physical = new PhysicalCopy();
            physical.IsSealed = ItemIsSealed;
            item = physical;
        }
        else if (ItemCopyType == "Підписка")
        {
            item = new SubscriptionCopy();
        }
        else
        {
            item = new DigitalCopy();
        }

        item.GameTitle = ItemGameTitle ?? "";
        item.Platform = ItemPlatform ?? "";
        item.Status = ItemStatus ?? "";
        item.Price = ItemPrice ?? 0;

        return item;
    }

    // Очищає форму для додавання нової копії.
    [RelayCommand]
    private void NewItem()
    {
        SelectedItem = null;
        ItemGameTitle = "";
        ItemPlatform = PlatformList[0];
        ItemCopyType = CopyTypeList[0];
        ItemPrice = 0;
        ItemStatus = StatusList[0];
        ItemIsSealed = false;
        ErrorMessage = "";
    }

    // Додає нову копію до колекції.
    [RelayCommand]
    private void AddItem()
    {
        RunSafely(() =>
        {
            CollectionItem item = ReadItemForm();

            _collection.AddItem(item);

            RefreshTables();

            StatusMessage = "Копію «" + item.GameTitle +
                "» (" + item.Platform + ") додано";

            Log.Information("Додано копію {GameTitle} на платформі {Platform}",
                item.GameTitle, item.Platform);
        });
    }

    // Зберігає зміни вибраної копії.
    [RelayCommand]
    private void SaveItem()
    {
        RunSafely(() =>
        {
            if (SelectedItem == null)
            {
                throw new ArgumentException("Спочатку оберіть копію в таблиці");
            }

            CollectionItem selected = SelectedItem;
            CollectionItem newItem = ReadItemForm();

            _collection.ReplaceItem(selected, newItem);

            RefreshTables();

            StatusMessage = "Копію «" + newItem.GameTitle +
                "» (" + newItem.Platform + ") змінено";

            Log.Information("Змінено копію {GameTitle} на платформі {Platform}",
                newItem.GameTitle, newItem.Platform);
        });
    }

    // Видаляє вибрану копію з колекції.
    [RelayCommand]
    private void DeleteItem()
    {
        RunSafely(() =>
        {
            if (SelectedItem == null)
            {
                throw new ArgumentException("Спочатку оберіть копію в таблиці");
            }

            CollectionItem selected = SelectedItem;
            string title = selected.GameTitle;
            string platform = selected.Platform;

            _collection.RemoveItem(selected);

            RefreshTables();
            NewItem();

            StatusMessage = "Копію «" + title +
                "» (" + platform + ") видалено";

            Log.Information("Видалено копію {GameTitle} з платформи {Platform}",
                title, platform);
        });
    }
}
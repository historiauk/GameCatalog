// ШІ: Claude Opus 5.5 (Medium) – методи FindGame, AddGame, ReplaceGame, RemoveGame – приклад-аналог (клас ProductCatalog) і таблиця замін; виправлено пропущені перейменування.
// Змінено: методи адаптовано студенткою під ігри і копії, коментарі переписано.
using System;
using System.Collections.Generic;

namespace GameCatalog.Models;

// Колекція користувача: список ігор каталогу і список копій
public class UserCollection
{
    public List<Game> Games { get; set; } = new List<Game>();
    public List<CollectionItem> Items { get; set; } = new List<CollectionItem>();

    // Знайти гру за назвою (великі/малі літери не важливі). Якщо немає – повертає null
    public Game? FindGame(string name)
    {
        foreach (Game game in Games)
        {
            if (game.Title.Trim().ToLower() == name.Trim().ToLower())
            {
                return game;
            }
        }
        return null;
    }

    // Додати гру, якщо такої ще немає
    public void AddGame(Game game)
    {
        if (FindGame(game.Title) != null)
        {
            throw new DuplicateGameException("Гра «" + game.Title + "» вже є");
        }
        Games.Add(game);
    }

    // Замінити стару гру новою (після редагування у формі)
    public void ReplaceGame(Game oldGame, Game newGame)
    {
        Game? sameName = FindGame(newGame.Title);
        if (sameName != null && sameName != oldGame)
        {
            throw new DuplicateGameException("Гра «" + newGame.Title + "» вже є");
        }

        // якщо назва змінилась – оновлюємо її і в колекції
        foreach (CollectionItem item in Items)
        {
            if (item.GameTitle == oldGame.Title)
            {
                item.GameTitle = newGame.Title;
            }
        }

        int index = Games.IndexOf(oldGame);
        Games[index] = newGame;
    }

    // Видалити гру разом з усією інформацією
    public void RemoveGame(Game game)
    {
        Games.Remove(game);
        Items.RemoveAll(item => item.GameTitle == game.Title);
    }
    // ШІ: Claude Opus 5.5 (Medium) – каркас методів HasItem, CheckItem, AddItem, ReplaceItem, RemoveItem з 9 пропусками.
    // Змінено: пропуски (умови, логічні оператори, тип винятку, операції зі списком) заповнила студентка.
    // Чи є вже в колекції ця гра на цій платформі (except – копія, яку зараз редагуємо, її не рахуємо)
    public bool HasItem(string gameTitle, string platform, CollectionItem? except)
    {
        foreach (CollectionItem item in Items)
        {
            if (item != except
            && item.GameTitle.Trim().ToLower() == gameTitle.Trim().ToLower()
            && item.Platform.Trim().ToLower() == platform.Trim().ToLower())
            {
                return true;
            }
        }
        return false;
    }

    // Перевірки перед тим, як додати або змінити копію
    private void CheckItem(CollectionItem item, CollectionItem? except)
    {
        if (FindGame(item.GameTitle) == null)
        {
            throw new ArgumentException("Гри «" + item.GameTitle + "» немає в каталозі");
        }

        if (string.IsNullOrWhiteSpace(item.Platform))
        {
            throw new ArgumentException("Оберіть платформу");
        }

        if (HasItem(item.GameTitle, item.Platform, except))
        {
            throw new DuplicateGameException("Гра «" + item.GameTitle + "» на платформі " + item.Platform + " вже є в колекції");
        }
    }

    public void AddItem(CollectionItem item)
    {
        CheckItem(item, null);
        Items.Add(item);
    }

    public void ReplaceItem(CollectionItem oldItem, CollectionItem newItem)
    {
        CheckItem(newItem, oldItem);
        int index = Items.IndexOf(oldItem);
        Items[index] = newItem;
    }

    public void RemoveItem(CollectionItem item)
    {
        Items.Remove(item);
    }
}
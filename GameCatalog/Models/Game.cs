// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (клас Book) і таблиця відповідностей; виправлено 4 помилки (поле _year, значення _title за замовчуванням, зайва дужка, назва інтерфейсу).
// Змінено: клас написано студенткою за прикладом під свою предметну область (назви, перевірки року і рейтингу, повідомлення помилок, ToString).
using System;

namespace GameCatalog.Models;

public class Game : IRateable
{
    private string _title = "";
    private int _year;
    private int _rating;

    public string Title
    {
        get { return _title; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Назва гри обов'язкова");
            }
            _title = value;
        }
    }
    
    public string Genre { get; set; } = "";
    
    public int Year
    {
        get { return _year; }
        set
        {
            if (value < 1970)
            {
                throw new ArgumentException("Рік має бути не раніше 1970");
            }
            _year = value;
        }
    }
    public int Rating
    {
        get { return _rating; }
        set
        {
            if (value < 1 || value > 10)
            {
                throw new ArgumentException("Рейтинг має бути від 1 до 10");
            }
            _rating = value;
        }
    }
    public override string ToString()
    {
        return Title + " (" + Year + ")";
    }
}
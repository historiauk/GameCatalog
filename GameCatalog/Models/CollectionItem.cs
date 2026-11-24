// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (абстрактний клас Vehicle) і таблиця замін.
// Змінено: клас адаптовано студенткою під копію гри (GameTitle, Platform, Status, GetValue), коментарі переписано.
using System;
using System.Text.Json.Serialization;

namespace GameCatalog.Models;

// Абстрактна позиція колекції: спільне для всіх типів копій
// ШІ: Claude Opus 5.5 (Medium) – атрибути JsonPolymorphic / JsonDerivedType.
// Змінено: без змін
// У JSON записується тип копії ("copyType"), щоб при читанні створити правильний клас-нащадок
[JsonPolymorphic(TypeDiscriminatorPropertyName = "copyType")]
[JsonDerivedType(typeof(DigitalCopy), "digital")]
[JsonDerivedType(typeof(PhysicalCopy), "physical")]
[JsonDerivedType(typeof(SubscriptionCopy), "subscription")]
public abstract class CollectionItem
{
    private string _gameTitle = "";
    private decimal _price;

    public string GameTitle
    {
        get { return _gameTitle; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Оберіть гру");
            }
            _gameTitle = value;
        }
    }

    public string Platform { get; set; } = "";
    public string Status { get; set; } = "";

    public decimal Price
    {
        get { return _price; }
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Ціна не може бути від'ємною");
            }
            _price = value;
        }
    }

    // Кожен нащадок МУСИТЬ написати свій розрахунок вартості
    public abstract decimal GetValue();

    // ШІ: Claude Opus 5.5 (Medium) – властивості TypeName і Value для показу в таблиці.
    // Змінено: без змін
    // Назва типу копії ("Цифрова", "Фізична", "Підписка") – кожен нащадок задає свою
    [JsonIgnore]
    public abstract string TypeName { get; }

    // Поточна вартість – для стовпця таблиці (таблиця не вміє показувати методи, лише властивості)
    [JsonIgnore]
    public decimal Value
    {
        get { return GetValue(); }
    }
}
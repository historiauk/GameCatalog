// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (клас Bicycle) і таблиця замін.
// Змінено: клас адаптовано студенткою (назва, власний розрахунок GetValue, коментар).
using System.Text.Json.Serialization;

namespace GameCatalog.Models;

// Підписка: гра не належить користувачу, вартість 0
public class SubscriptionCopy : CollectionItem
{
    // ШІ: Claude Opus 5.5 (Medium) – властивість TypeName для стовпця таблиці.
    // Змінено: без змін.
    [JsonIgnore]
    public override string TypeName { get { return "Підписка"; } }

    public override decimal GetValue()
    {
        return 0;
    }
}
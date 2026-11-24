// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (клас Car) і таблиця замін.
// Змінено: клас адаптовано студенткою (назва, власний розрахунок GetValue, коментар).
using System.Text.Json.Serialization;

namespace GameCatalog.Models;

// Цифрова копія: вартість дорівнює ціні
public class DigitalCopy : CollectionItem
{
    // ШІ: Claude Opus 5.5 (Medium) – властивість TypeName для стовпця таблиці.
    // Змінено: без змін.
    [JsonIgnore]
    public override string TypeName { get { return "Цифрова"; } }

    public override decimal GetValue()
    {
        return 	Price;
    }
}
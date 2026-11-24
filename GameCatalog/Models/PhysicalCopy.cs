// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (клас Scooter) і таблиця замін.
// Змінено: клас адаптовано студенткою (назва, власний розрахунок GetValue, коментар).
using System.Text.Json.Serialization;

namespace GameCatalog.Models;

// Фізична копія: запакована коштує в 1,5 раза дорожче
public class PhysicalCopy : CollectionItem
{
    public bool IsSealed { get; set; }

    // ШІ: Claude Opus 5.5 (Medium) – властивість TypeName для стовпця таблиці.
    // Змінено: без змін.
    [JsonIgnore]
    public override string TypeName { get { return "Фізична"; } }

    public override decimal GetValue()
    {
        if (IsSealed)
        {
            return Price * 1.5m;
        }
        return Price;
    }
}
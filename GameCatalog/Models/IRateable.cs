// ШІ: Claude Opus 5.5 (Medium) – приклад-аналог (інтерфейс IPriced); виправлено назву інтерфейсу.
// Змінено: інтерфейс написано студенткою за прикладом, вимога змінена на int Rating.
namespace GameCatalog.Models;

public interface IRateable
{
    int Rating { get; set; }
}
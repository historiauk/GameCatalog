namespace GameCatalog.Models;

// Зберігає спільні налаштування каталогу в одному місці.
public static class CatalogOptions
{
    // Статус, який позначає завершену гру.
    public const string CompletedStatus = "Пройдено";

    // Список доступних ігрових платформ.
    public static readonly string[] Platforms =
    {
        "PC",
        "PlayStation 5",
        "PlayStation 4",
        "Xbox Series X|S",
        "Nintendo Switch"
    };

    // Список можливих статусів проходження гри.
    public static readonly string[] Statuses =
    {
        "Не почато",
        "Граю",
        "Пройдено",
        "Закинуто"
    };

    // Список типів придбання гри.
    public static readonly string[] CopyTypes =
    {
        "Цифрова",
        "Фізична",
        "Підписка"
    };
}
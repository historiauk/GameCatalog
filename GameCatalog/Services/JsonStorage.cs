using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using GameCatalog.Models;
using Serilog;

namespace GameCatalog.Services;

public class JsonStorage
{
    // Зберігає налаштування читання та запису JSON.
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        // Робить JSON-файл зручним для читання, додаючи відступи.
        WriteIndented = true,

        // Записує назви властивостей у форматі camelCase.
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

        // Дозволяє читати назви властивостей незалежно від великих і малих літер.
        PropertyNameCaseInsensitive = true,

        // Дозволяє зберігати кирилицю без перетворення на спеціальні коди.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        // Дозволяє метаданим типу копії розташовуватися не лише на початку об'єкта.
        AllowOutOfOrderMetadataProperties = true
    };

    // Завантажує колекцію з JSON-файлу.
    public UserCollection Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл не знайдено: " + path);
        }

        try
        {
            // Читаємо весь вміст файлу.
            string json = File.ReadAllText(path);

            // Перетворюємо JSON-текст на об'єкт колекції.
            UserCollection? collection = JsonSerializer.Deserialize<UserCollection>(json, Options);

            if (collection == null)
            {
                throw new InvalidDataException("Файл порожній");
            }

            Log.Information("Прочитано файл {Path}: ігор {Games}, копій {Items}",
                path, collection.Games.Count, collection.Items.Count);

            return collection;
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Помилка структури JSON-файлу {Path}", path);
            throw new InvalidDataException("Файл пошкоджений: " + ex.Message, ex);
        }
        catch (NotSupportedException ex)
        {
            Log.Error(ex, "Невідомий тип копії у файлі {Path}", path);
            throw new InvalidDataException("Файл містить невідомий тип копії: " + ex.Message, ex);
        }
        catch (ArgumentException ex)
        {
            Log.Error(ex, "Некоректні дані у файлі {Path}", path);
            throw new InvalidDataException("Файл містить некоректні дані: " + ex.Message, ex);
        }
    }

    // Зберігає колекцію у JSON-файл.
    public void Save(UserCollection collection, string path)
    {
        // Отримуємо шлях до папки та створюємо її, якщо потрібно.
        string? folder = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Перетворюємо об'єкт колекції на JSON-текст.
        string json = JsonSerializer.Serialize(collection, Options);

        // Записуємо JSON-текст у файл.
        File.WriteAllText(path, json);

        Log.Information("Збережено файл {Path}: ігор {Games}, копій {Items}",
            path, collection.Games.Count, collection.Items.Count);
    }
}
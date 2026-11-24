using System.IO;
using System.Xml.Serialization;
using GameCatalog.Models;

namespace GameCatalog.Services;

public class XmlExporter
{
    // Експортує статистику колекції у файл XML.
    public void Export(CollectionStatistics statistics, string path)
    {
        // Отримуємо шлях до папки та створюємо її, якщо потрібно.
        string? folder = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // Створюємо серіалізатор для перетворення статистики у XML.
        XmlSerializer serializer = new XmlSerializer(typeof(CollectionStatistics));

        // Відкриваємо файл для запису та зберігаємо статистику.
        using (FileStream stream = File.Create(path))
        {
            serializer.Serialize(stream, statistics);
        }
    }
}
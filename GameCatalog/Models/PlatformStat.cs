// ШІ: Claude Opus 5.5 (Medium) – каркас класу з пропусками; пропуски заповнила студентка.
using System.Xml.Serialization;

namespace GameCatalog.Models;

// Статистика колекції для однієї платформи (записується у XML)
public class PlatformStat
{
    [XmlAttribute("name")]
    public string Platform { get; set; } = "";

    public int ItemCount { get; set; }        // скільки копій на платформі
    public int CompletedCount { get; set; }   // скільки з них пройдено
    public decimal TotalPrice { get; set; }       // сума цін
    public decimal TotalValue { get; set; }       // сума вартостей GetValue()

    // Додати результати іншої частини (знадобиться для багатопотоковості)
    public void Add(PlatformStat other)
    {
        ItemCount += other.ItemCount;
        CompletedCount += other.CompletedCount;
        TotalPrice += other.TotalPrice;
        TotalValue += other.TotalValue;
    }
}

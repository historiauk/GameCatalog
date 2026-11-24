// ШІ: Claude Opus 5.5 (Medium) – каркас класу з пропусками; пропуски заповнила студентка.
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameCatalog.Models;

// Уся статистика колекції – саме цей об'єкт записується у файл XML
[XmlRoot("CollectionStatistics")]
public class CollectionStatistics
{
    public DateTime CreatedAt { get; set; }   // коли пораховано
    public int TotalItems { get; set; }        // скільки всього копій

    [XmlArray("Platforms")]
    [XmlArrayItem("PlatformStat")]
    public List<PlatformStat> Platforms { get; set; } = new List<PlatformStat>();
}

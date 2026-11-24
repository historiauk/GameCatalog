// ШІ: Claude Opus 5.5 (Medium) – клас ChartBar (один стовпчик діаграми на сторінці «Огляд»).
// Змінено: без змін
using Avalonia.Media;

namespace GameCatalog.ViewModels;

// Один стовпчик або смужка діаграми: підпис, число, розмір у пікселях і колір
public class ChartBar
{
    public string Label { get; set; } = "";
    public string ValueText { get; set; } = "";
    public double Size { get; set; }
    public IBrush Brush { get; set; } = Brushes.Cyan;
}

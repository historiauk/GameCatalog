// ШІ: Claude Opus 5.5 (Medium) – клас власного винятку надано повністю.
// Змінено: без змін.
using System;

namespace GameCatalog.Models;

public class DuplicateGameException : Exception
{
    public DuplicateGameException(string message) : base(message)
    {
    }
}
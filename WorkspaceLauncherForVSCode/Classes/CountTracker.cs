using System;
using System.Collections.Generic;

namespace WorkspaceLauncherForVSCode.Classes;
public class CountTracker
{
  private readonly Dictionary<CountType, int> _counts = new();

  public event EventHandler<CountType>? CountChanged;

  public int this[CountType type] => _counts.TryGetValue(type, out var count) ? count : 0;

  public void Update(CountType type, int value)
  {
    if (!_counts.TryGetValue(type, out var oldValue) || oldValue != value)
    {
      _counts[type] = value;
      CountChanged?.Invoke(this, type);
    }
  }
}

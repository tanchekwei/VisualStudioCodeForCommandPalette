using System.Collections.Generic;
using System.Linq;
using WorkspaceLauncherForVSCode.Enums;

namespace WorkspaceLauncherForVSCode.Classes;

public class CountTracker
{
  private readonly Dictionary<CountType, int> _counts = new();
  private readonly Dictionary<VisualStudioCodeRemoteType, int> _vscodeRemoteCounts = new();
  private readonly Dictionary<WorkspaceType, int> _vscodeLocalCounts = new();
  public int this[CountType type] => _counts.TryGetValue(type, out var count) ? count : 0;
  public int this[VisualStudioCodeRemoteType remoteType] => _vscodeRemoteCounts.TryGetValue(remoteType, out var count) ? count : 0;
  public int this[WorkspaceType localType] => _vscodeLocalCounts.TryGetValue(localType, out var count) ? count : 0;

  public void Update(CountType type, int value)
  {
    if (!_counts.TryGetValue(type, out var oldValue) || oldValue != value)
    {
      _counts[type] = value;
    }
  }

  public void Update(VisualStudioCodeRemoteType remoteType, int value)
  {
    if (!_vscodeRemoteCounts.TryGetValue(remoteType, out var oldValue) || oldValue != value)
    {
      _vscodeRemoteCounts[remoteType] = value;
    }
  }

  public void Update(WorkspaceType localType, int value)
  {
    if (!_vscodeLocalCounts.TryGetValue(localType, out var oldValue) || oldValue != value)
    {
      _vscodeLocalCounts[localType] = value;
    }
  }

  public void Reset()
  {
    foreach (var key in _counts.Keys.ToList())
    {
      _counts[key] = 0;
    }

    foreach (var key in _vscodeRemoteCounts.Keys.ToList())
    {
      _vscodeRemoteCounts[key] = 0;
    }

    foreach (var key in _vscodeLocalCounts.Keys.ToList())
    {
      _vscodeLocalCounts[key] = 0;
    }
  }
}

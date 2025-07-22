using System;
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
        try
        {
            if (!_counts.TryGetValue(type, out var oldValue) || oldValue != value)
            {
                _counts[type] = value;
            }
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }

    public void Increment(CountType type)
    {
        try
        {
            _counts.TryGetValue(type, out var value); _counts[type] = value + 1;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }

    public void Increment(VisualStudioCodeRemoteType remoteType)
    {
        try
        {
            _vscodeRemoteCounts.TryGetValue(remoteType, out var value);
            _vscodeRemoteCounts[remoteType] = value + 1;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }

    public void Increment(WorkspaceType localType)
    {
        try
        {
            _vscodeLocalCounts.TryGetValue(localType, out var value); _vscodeLocalCounts[localType] = value + 1;
        }
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }

    public void Reset()
    {
        try
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
        catch (Exception ex)
        {
            ErrorLogger.LogError(ex);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 取得した強化を保持
/// </summary>
public class EnhanceInventory 
{
    private Dictionary<EnhanceData, int> _levels = new();

    public int GetLevel(EnhanceData data) => _levels.TryGetValue(data, out var lv) ? lv : 0;

    public void Add(EnhanceData date)
    {
        if(!_levels.ContainsKey(date))
        {
            _levels[date] = 0;
        }
        _levels[date]++;
    }

    public IEnumerable<EnhanceData> GetAllAcquiredData()
    {
        return _levels.Keys;
    }
}

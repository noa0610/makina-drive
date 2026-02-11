using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 画面外のランダムな方向からユニットを生成
/// </summary>
[Serializable]
public class SpawnRandom : ISpawnComponent
{
    public GameObject target { get; set; }
    
    public List<Vector3> GetPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        for(int i = 0; i < count; i++)
        {
            positions.Add(SpawnUtils.GetRandomOffScreenPosition(target));
        }
        return positions;
    }

    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        foreach (var unit in pool)
        {
            unit.transform.position = SpawnUtils.GetRandomOffScreenPosition(target);
        }
        return pool;
    }

    public void ApplyParameters(string paramString)
    {
        if (string.IsNullOrEmpty(paramString)) return;
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ランダムな方向からユニットを生成
/// </summary>
[Serializable]
public class SpawnRandom : ISpawnComponent
{
    public GameObject target { get; set; }
    public SpawnRandom() { }

    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        foreach (var unit in pool)
        {
            unit.transform.position = SpawnUtils.GetRandomOffScreenPosition(target);
        }
        return pool;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/WaveData")]
public class WaveData : ScriptableObject
{
    public string waveName;
    public List<UnitSpawnInfo> spawnInfos = new List<UnitSpawnInfo>();
}

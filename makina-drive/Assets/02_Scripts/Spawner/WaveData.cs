using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    public string waveName;
    public List<UnitSpawnInfo> spawnInfos = new List<UnitSpawnInfo>();
}

using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "MakinaDrive/WaveDataList")]
public class WaveDataList : ScriptableObject
{
    public List<WaveData> waveDatas = new List<WaveData>();
}

using System;
using UnityEngine;

/// <summary>
/// 生成情報
/// </summary>
[Serializable]
public class UnitSpawnInfo
{
    public UnitBase unitBase;    // 生成する敵

    [SerializeReference, SubclassSelector]
    public ISpawnComponent comp;       // 生成パターン
    public int spawnCount;             // スポーン回数
    public int sameTimeSpawnCount = 1; // 同時スポーン数
    public float spawnTime;            // 生成開始時間
    public float spawnEndTime;         // 生成終了時間(0で未指定)
    public float spawnInterval = 1;    // 生成間隔
    public float statusRate;           // ステータス強化倍率
    public float destroyTime;          // 生成後に消去する時間指定（0で消去しない）
}

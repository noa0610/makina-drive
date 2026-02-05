using System;
using System.Collections.Generic;
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

    [Tooltip("一度に生成される最小数")]
    public int minSpawnCount = 1;      // 同時最小スポーン数
    
    [Tooltip("一度に生成される最大数")]
    public int maxSpawnCount = 1;      // 同時最大スポーン数

    public float spawnTime;            // 生成開始時間
    public float spawnEndTime;         // 生成終了時間(0で未指定)
    public float spawnInterval = 1;    // 生成間隔

    public List<StatusOverride> statusOverrides = new List<StatusOverride>();
    public float statusRate;           // ステータス強化倍率
    public float destroyTime;          // 生成後に消去する時間指定（0で消去しない）

    public string spawneEffectName;    // 敵生成時のエフェクト名
    public string FirstSpawneSEName;         // 最初の生成時のSE名

    [Header("クリア条件設定")]
    public bool isClearTarget; // この設定をした敵をすべて倒すとクリア
}

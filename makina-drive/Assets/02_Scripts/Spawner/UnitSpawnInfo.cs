using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成情報
/// </summary>
[Serializable]
public class UnitSpawnInfo
{
    [Header("生成情報設定")]
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

    [Tooltip("予告エフェクト発生から敵が生成されるまでの時間")]
    public float spawnDelay = 1.0f;   // 生成ディレイ時間

    public List<StatusOverride> statusOverrides = new List<StatusOverride>();
    public float waveIncreaseRate;     // ウェーブごとのステータス強化倍率
    public float destroyTime;          // 生成後に消去する時間指定（0で消去しない）

    [Header("エフェクト設定")]
    public string previewEffectName;   // 生成予告のエフェクト名
    public string spawneEffectName;    // 敵生成時のエフェクト名

    [Header("SE設定")]
    public string FirstSpawneSEName;   // 最初の生成時のSE名

    [Header("クリア条件設定")]
    public bool isClearTarget; // この設定をした敵をすべて倒すとクリア
}

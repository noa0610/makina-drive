using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターゲット指定されたオブジェクト周囲に敵を生成
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _targetObject; // プレイヤー
    [Header("敵生成情報リスト")]
    [SerializeField] private List<UnitSpawnInfo> spawnInfos = new List<UnitSpawnInfo>();
    private float _elapsedTime = 0;

    // 各スポーン設定ごとのタイマーを保持
    private List<float> _spawnTimers = new List<float>();
    private List<int> _currentSpawnCounts = new List<int>();
    private List<bool> _isFinished = new List<bool>();

    private void Start()
    {
        // 管理用リストを初期化
        foreach (var info in spawnInfos)
        {
            _spawnTimers.Add(0f);
            _currentSpawnCounts.Add(0);
            _isFinished.Add(false);
        }
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        for (int i = 0; i < spawnInfos.Count; i++)
        {
            if (_isFinished[i]) continue;

            var info = spawnInfos[i];

            // ～強制終了時間のチェック～
            if (_elapsedTime >= info.spawnEndTime && info.spawnEndTime != 0)
            {
                _isFinished[i] = true;
                Debug.Log($"{info.unitBase.name} の生成フェーズが終了時間に達しました。");
                continue;
            }

            // ～生成開始時間のチェック～
            if (_elapsedTime < info.spawnTime) continue;

            _spawnTimers[i] += Time.deltaTime;

            // ～スポーン間隔のチェック～
            if (_spawnTimers[i] >= info.spawnInterval)
            {
                SpawnGroup(i, info);
                _spawnTimers[i] = 0;

                // ～生成数のチェック～
                if (_currentSpawnCounts[i] >= info.spawnCount)
                {
                    _isFinished[i] = true;
                    Debug.Log($"{info.unitBase.name} が指定数（{info.spawnCount}体）に達したため終了します。");
                }
            }
        }
    }

    /// <summary>
    /// 敵生成処理
    /// </summary>
    private void SpawnGroup(int index, UnitSpawnInfo info)
    {
        if (info.unitBase == null) return;

        List<UnitBase> groupList = new List<UnitBase>();

        for (int j = 0; j < info.sameTimeSpawnCount; j++)
        {
            UnitBase unit = Instantiate(info.unitBase);
            
            // TODO UnitBase内に倍率を受け取りステータスを強化する処理を用意する、現在未実装
            // 敵の強化ロジック（適当な例）
            // float powerMultiplier = 1.0f + (_elapsedTime * info.statusRate);
            // unit.ApplyStatusMultiplier(powerMultiplier); 

            if (info.destroyTime > 0) Destroy(unit.gameObject, info.destroyTime);
            
            groupList.Add(unit);
        }

        // 実行回数をカウント
        _currentSpawnCounts[index]++;

        // まとめて配置を実行
        info.comp.target = _targetObject;
        info.comp.Execute(groupList);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// ターゲット指定されたオブジェクト周囲に敵を生成
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public event Action<int> OnWaveChange;               // ウェーブ数変更イベント
    [SerializeField] private GameObject _targetObject;   // プレイヤー
    [SerializeField] private float _waveDuration = 60f;  // 1ウェーブの時間
    [SerializeField] private int _currentWaveNumber = 1; // ウェーブ数
    [SerializeField] private TimerCount _timer;
    [SerializeField] private List<Status> _statusUp = new List<Status>();

    [Header("敵生成情報リスト")]
    [SerializeField] private List<WaveData> _normalWaves = new List<WaveData>();

    [Header("エンドレスウェーブ設定")]
    [SerializeField] private List<WaveData> _endlessWaves = new List<WaveData>();

    private float _elapsedTime = 0;         // 経過時間
    private int _totalClearTargetCount = 0;  // 必要撃破数
    private int _currentClearTargetKill = 0; // 現在の撃破数
    private bool _isCleared = false;

    // 現在のウェーブ内での敵生成進捗
    private List<float> _spawnTimers = new List<float>();
    private List<int> _currentSpawnCounts = new List<int>();

    private void Start()
    {
        if (_timer == null)
        {
            Debug.Log("タイマーを指定してください。");
        }

        CalculateTotalClearTargets();

        // ユニット死亡イベント購読
        UnitBase.OnAnyUnitDeath += HandleUnitDeath;
    }

    private void OnDestroy()
    {
        UnitBase.OnAnyUnitDeath -= HandleUnitDeath;
    }

    // 設定されたクリアターゲットの数を保有
    private void CalculateTotalClearTargets()
    {
        _totalClearTargetCount = 0;
        foreach (var wave in _normalWaves)
        {
            foreach (var info in wave.spawnInfos)
            {
                if (info.isClearTarget)
                {
                    _totalClearTargetCount += info.spawnCount * info.sameTimeSpawnCount;
                }
            }
        }
        Debug.Log($"クリアに必要な撃破数：{_totalClearTargetCount}");
    }

    // クリアターゲットの判別、撃破数をカウント
    private void HandleUnitDeath(UnitBase unit)
    {
        if (unit.IsClearTarget)
        {
            _currentClearTargetKill++;
            Debug.Log($"クリア対象撃破 現在：{_currentClearTargetKill} / {_totalClearTargetCount}");

            if (_currentClearTargetKill >= _totalClearTargetCount)
            {
                GameClear();
            }
        }
    }

    private void GameClear()
    {
        if (!_isCleared)
        {
            Debug.Log($"ゲームクリア");
            GameStateManager.instance.ChangeState(GameState.Clear);
        }
        _isCleared = true;
    }

    private void Update()
    {
        if (_timer != null)
        {
            _elapsedTime = _timer.currentTime;
        }
        // else
        // {
        //     _elapsedTime += Time.deltaTime;
        // }

        int newWaveNumber = Mathf.FloorToInt(_elapsedTime / _waveDuration) + 1;

        if (newWaveNumber != _currentWaveNumber)
        {
            _currentWaveNumber = newWaveNumber;
            OnWaveChange?.Invoke(_currentWaveNumber);
            ResetWaveProgress();
        }

        ProcessCurrentWave();
    }

    // 次のウェーブ初期化
    private void ResetWaveProgress()
    {
        _spawnTimers.Clear();
        _currentSpawnCounts.Clear();

        WaveData currentWave = GetCurrentWaveData();
        foreach (var info in currentWave.spawnInfos)
        {
            _spawnTimers.Add(0f);
            _currentSpawnCounts.Add(0);
        }
    }

    // ウェーブリストのデータ取得
    private WaveData GetCurrentWaveData()
    {
        int index = _currentWaveNumber - 1;

        // 通常ウェーブの範囲内か
        if (index < _normalWaves.Count)
        {
            return _normalWaves[index];
        }
        else
        {
            // エンドレス用のリスト内でループさせる
            int endlessIndex = (index - _normalWaves.Count) % _endlessWaves.Count;
            return _endlessWaves[endlessIndex];
        }
    }

    // 現在のウェーブの生成確認処理
    private void ProcessCurrentWave()
    {
        WaveData currentWave = GetCurrentWaveData();
        float timeInWave = _elapsedTime % _waveDuration;

        if (_spawnTimers.Count != currentWave.spawnInfos.Count)
        {
            ResetWaveProgress();
        }

        for (int i = 0; i < currentWave.spawnInfos.Count; i++)
        {
            var info = currentWave.spawnInfos[i];

            // 生成終了時間を過ぎたか、生成回数上限の場合はスキップ
            if ((info.spawnEndTime > 0 && timeInWave >= info.spawnEndTime) || _currentSpawnCounts[i] >= info.spawnCount) continue;

            // 生成開始時間のチェック
            if (timeInWave < info.spawnTime) continue;

            _spawnTimers[i] += Time.deltaTime;

            if (_spawnTimers[i] >= info.spawnInterval)
            {
                SpawnGroup(i, info);
                _spawnTimers[i] = 0;
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

        float powerMultiplier = _currentWaveNumber * info.statusRate;

        for (int j = 0; j < info.sameTimeSpawnCount; j++)
        {
            UnitBase unit = Instantiate(info.unitBase);

            // ウェーブ数に応じて経験値増加
            unit.DropExp = info.unitBase.UnitStatusData.baseExp * (1 + _currentWaveNumber * info.statusRate);

            // ウェーブ数に応じた強化倍率の計算
            // 例、statusRate = 0.1 → ウェーブ2で0.1（1.1倍の強化）、ウェーブ10で0.9 (1.9倍の強化)
            float currentWaveMultiplier = (_currentWaveNumber - 1) * info.statusRate;

            if (currentWaveMultiplier > 0)
            {
                unit.ApplyWaveStatus(currentWaveMultiplier, _statusUp);
            }

            if (info.destroyTime > 0) unit.SetLazyDeath(info.destroyTime);

            // クリアフラグ付与
            unit.IsClearTarget = info.isClearTarget;

            groupList.Add(unit);
        }

        // 実行回数をカウント
        _currentSpawnCounts[index]++;

        // まとめて配置を実行
        info.comp.target = _targetObject;
        info.comp.Execute(groupList);
    }
}

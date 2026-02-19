using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
# endif

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
    [SerializeField] private VisualInfo _clearTargetKillSE;
    [SerializeField] private string _clearTargetKillEffect;
    [SerializeField] private VisualInfo _clearSE;
    [SerializeField] private bool _showHPBar = false; // HPバーの表示
    [SerializeField] private string _defaultPreviewEffect; // 基本の生成予告エフェクト
    [SerializeField] private List<Status> _statusUp = new List<Status>();

    [Header("敵生成情報リスト")]
    [SerializeField] private List<WaveData> _normalWaves = new List<WaveData>();

    [Header("エンドレス敵生成情報リスト")]
    [SerializeField] private List<WaveData> _endlessWaves = new List<WaveData>();


#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool _notStatusUP = false;
# endif

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
                    _totalClearTargetCount += info.processCount * info.minSpawnCount;
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
            if (SoundManager.instance && _clearTargetKillSE.SEName != null)
            {
                SoundManager.instance.PlaySE(_clearTargetKillSE.SEName, _clearTargetKillSE.Volume);
                Debug.Log("クリア対象撃破SE");
            }

            if(EffectManager.instance && !string.IsNullOrEmpty(_clearTargetKillEffect))
            {
                EffectManager.instance.Play(_clearTargetKillEffect, unit.transform.position);
            }

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

            if (SoundManager.instance)
            {
                SoundManager.instance.AllStopBGM();
                if (_clearSE.SEName != null) SoundManager.instance.PlaySE(_clearSE.SEName, _clearSE.Volume);
            }
        }
        _isCleared = true;
    }

    private void Update()
    {
        if (_isCleared) return;

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
            if (_endlessWaves == null) return null;

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

            // 処理終了時間を過ぎたか、処理回数上限の場合はスキップ
            if ((info.processEndTime > 0 && timeInWave >= info.processEndTime) || _currentSpawnCounts[i] >= info.processCount) continue;

            // 処理開始時間のチェック
            if (timeInWave < info.processStartTime) continue;

            _spawnTimers[i] += Time.deltaTime;

            if (_spawnTimers[i] >= info.processInterval)
            {
                ProcessSpawnStep(info, i).Forget();
                _spawnTimers[i] = 0;
            }
        }
    }

    //     /// <summary>
    //     /// 敵生成処理
    //     /// </summary>
    //     private void SpawnGroup(int index, UnitSpawnInfo info)
    //     {
    //         if (info.unitBase == null) return;

    //         int randomSpawnCount = UnityEngine.Random.Range(info.minSpawnCount, info.maxSpawnCount + 1);

    //         List<UnitBase> spawnGroup = new List<UnitBase>(); // 生成する敵を格納する一時リスト

    //         for (int j = 0; j < randomSpawnCount; j++)
    //         {
    //             UnitBase unit = Instantiate(info.unitBase);

    //             // ウェーブ数に応じた強化倍率の計算
    //             // 例： statusRate = 0.1 → ウェーブ2で0.2（1.2倍の強化）、ウェーブ10で1.0 (2.0倍の強化)
    //             float waveProgression = _currentWaveNumber * info.waveIncreaseRate;

    //             // エディターでのみステータス強化のON/OFF可、ビルド後は常に強化を適用
    //             bool shouldApplyStatus = true;
    // # if UNITY_EDITOR
    //             if (_notStatusUP) shouldApplyStatus = false;

    // #endif
    //             // ステータス強化処理
    //             if (shouldApplyStatus && waveProgression > 0)
    //             {
    //                 List<StatusOverride> currentOverrrides = new List<StatusOverride>();

    //                 foreach (var so in info.statusOverrides)
    //                 {
    //                     StatusOverride calculatedSo = new StatusOverride
    //                     {
    //                         type = so.type,
    //                         // 倍率 = 1.0 + (設定された増加分 * ウェーブ進行度)
    //                         // 例： multiplierが1.2(0.2増)なら、ウェーブ数 * 0.2 となる
    //                         multiplier = 1f + ((so.multiplier - 1f) * (1f + waveProgression)),
    //                         addition = so.addition * (1f + waveProgression)
    //                     };
    //                     currentOverrrides.Add(calculatedSo);
    //                 }

    //                 // 強化量を適用
    //                 if (currentOverrrides.Count > 0)
    //                 {
    //                     unit.ApplyWaveStatus(currentOverrrides);
    //                 }
    //                 else if (info.waveIncreaseRate > 0)
    //                 {
    //                     if (_statusUp == null || _statusUp.Count == 0)
    //                     {
    //                         Debug.Log("強化するステータスが設定されていません。強化対象としてMaxHP, Atk, Speed, DashSpeedを指定します。");
    //                         _statusUp = new List<Status>{
    //                             Status.MaxHP,
    //                             Status.ATK,
    //                             Status.Speed,
    //                             Status.DashSpeed
    //                         };
    //                     }
    //                     unit.ApplyWaveStatus(waveProgression, _statusUp);
    //                 }
    //             }

    //             // ウェーブ数に応じて経験値増加
    //             unit.DropExp = info.unitBase.UnitStatusData.baseExp * (1 + _currentWaveNumber * info.waveIncreaseRate);

    //             // HPバーを生成
    //             if (UnitManager.instance != null) UnitManager.instance.CreateHPBar(unit, _showHPBar);

    //             // 0以上の値であれば時間経過で死亡させる
    //             if (info.destroyTime > 0) unit.SetLazyDeath(info.destroyTime);

    //             // クリアフラグ付与
    //             unit.IsClearTarget = info.isClearTarget;

    //             spawnGroup.Add(unit);
    //         }

    //         // 実行回数をカウント
    //         _currentSpawnCounts[index]++;

    //         if (info.comp != null && spawnGroup.Count > 0)
    //         {
    //             // まとめて配置を実行
    //             info.comp.target = _targetObject;
    //             info.comp.Execute(spawnGroup);
    //         }
    //     }

    /// <summary>
    /// 設定された数の敵を生成
    /// </summary>
    /// <param name="info"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private async UniTaskVoid ProcessSpawnStep(UnitSpawnInfo info, int index)
    {
        // 生成数を決定
        int count = UnityEngine.Random.Range(info.minSpawnCount, info.maxSpawnCount + 1);

        // ターゲットを指定
        info.comp.target = _targetObject;

        // 全個体分の生成座標を取得
        List<Vector3> spawnPositions = info.comp.GetPositions(count);

        // 各座標に対して生成処理を実行
        foreach (var pos in spawnPositions)
        {
            SpawnIndividualUnit(info, pos).Forget();
        }

        // 実行回数をカウント
        _currentSpawnCounts[index]++;
    }

    /// <summary>
    /// 敵個体ごとのの生成予告、生成フロー処理
    /// </summary>
    /// <param name="info"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private async UniTaskVoid SpawnIndividualUnit(UnitSpawnInfo info, Vector3 position)
    {
        // 予告エフェクトの再生（生成情報側を優先）
        string previewEffect = !string.IsNullOrEmpty(info.previewEffectName) ? info.previewEffectName : _defaultPreviewEffect;
        if (!string.IsNullOrEmpty(previewEffect))
        {
            PlayEffect(previewEffect, position);
        }

        // SE再生
        if (info.FirstProcessSEName != null && SoundManager.instance != null)
        {
            SoundManager.instance.PlaySE(info.FirstProcessSEName, info.FirstProcessSEVolume);
        }

        // 指定時間待機
        if (info.spawnDelay > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(info.spawnDelay), cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        // ユニットの生成と初期化
        UnitBase unit = Instantiate(info.unitBase, position, Quaternion.identity);

        // 出現エフェクト
        if (!string.IsNullOrEmpty(info.spawneEffectName))
        {
            PlayEffect(info.spawneEffectName, position);
        }


        // 初期化処理の実行
        SetUpUnit(unit, info);
    }


    /// <summary>
    /// ユニットの初期化処理（ステータス設定など）
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="info"></param>
    private void SetUpUnit(UnitBase unit, UnitSpawnInfo info)
    {
        // エディターでのみステータス強化のON/OFF可、ビルド後は常に強化を適用
        bool shouldApplyStatus = true;
#if UNITY_EDITOR
        if (_notStatusUP) shouldApplyStatus = false;
#endif
        // ウェーブに応じたステータス強化
        if (shouldApplyStatus)
        {
            if (info.statusOverrides != null && info.statusOverrides.Count > 0)
            {
                unit.ApplyWaveStatus(info.statusOverrides);
            }
            else
            {
                unit.ApplyWaveStatus(CalculateWaveMultiplier(info), _statusUp);
                Debug.Log("旧強化ロジックが呼ばれています。");
            }
        }

        // 経験値設定
        float baseExp = info.unitBase.UnitStatusData.baseExp;
        // (基礎経験値 + ウェーブ加算) * (1 + ウェーブ倍率)
        float waveBonus = (_currentWaveNumber - 1) * info.expAdditionPerWave;
        float waveMultiplier = 1f * ((_currentWaveNumber - 1) * info.expMultiplierPerWave);
        unit.DropExp = (baseExp + waveBonus) * waveMultiplier;
        // Debug.Log($"{unit.name} Exp: {unit.DropExp} (Base:{baseExp}, Bonus:{waveBonus}, Mult:{waveMultiplier})");

        // HPバー
        if (UnitManager.instance != null)
            UnitManager.instance.CreateHPBar(unit, _showHPBar);

        // 時限消去
        if (info.destroyTime > 0)
            unit.SetLazyDeath(info.destroyTime);

        // クリア対象フラグ
        unit.IsClearTarget = info.isClearTarget;
    }

    /// <summary>
    /// 旧強化ロジック
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private float CalculateWaveMultiplier(UnitSpawnInfo info)
    {
        return 1 + (_currentWaveNumber - 1) * info.waveIncreaseRate;
    }

    /// <summary>
    /// エフェクトの再生
    /// </summary>
    /// <param name="effectName"></param>
    /// <param name="position"></param>
    private void PlayEffect(string effectName, Vector3 position)
    {
        if (EffectManager.instance != null)
        {
            EffectManager.instance.Play(effectName, position);
        }
    }

}

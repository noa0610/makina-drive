using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// チュートリアル用敵生成管理クラス
/// </summary>
public class TutorialEnemySpawner : MonoBehaviour
{
    private List<UnitBase> _spawnedUnits = new List<UnitBase>();
    private CancellationTokenSource _spawnCts;
    private bool _isSpawning = false; // 生成中

    [SerializeField] private bool _showHPBar = false; // HPバーの表示
    [SerializeField] private string _defaultPreviewEffect; // 共通の予告エフェクト

    public event Action<UnitBase> OnEnemyDefeated; // 個別の敵撃破イベント
    public event Action OnAllEnemyDead; // 敵全滅イベント


    // 生成開始
    public void StartSpawn(UnitSpawnInfo info, GameObject target)
    {
        StopSpawning();
        _spawnCts = new CancellationTokenSource();

        if (info.unitBase == null) return;

        _isSpawning = true; // 生成開始
        SpawneLoop(info, target, _spawnCts.Token).Forget();
    }

    private async UniTaskVoid SpawneLoop(UnitSpawnInfo info, GameObject target, CancellationToken ct)
    {
        for (int i = 0; i < info.processCount; i++)
        {
            if (ct.IsCancellationRequested) break;

            SpawnGroupAsynk(info, target, ct).Forget();

            if (i < info.processCount - 1)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(info.processInterval), cancellationToken: ct);
            }
        }
        _isSpawning = false; // 生成終了

        // 全滅済みの場合の処理
        // if (_spawnedUnits.Count == 0) OnAllEnemyDead?.Invoke();
    }

    /// <summary>
    /// エネミー生成処理
    /// </summary>
    /// <param name="info"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private async UniTaskVoid SpawnGroupAsynk(UnitSpawnInfo info, GameObject target, CancellationToken ct)
    {
        int randomSpawnCount = UnityEngine.Random.Range(info.minSpawnCount, info.maxSpawnCount + 1);

        // ターゲットを指定
        info.comp.target = target;

        // 全個体分の生成座標を取得
        List<Vector3> spawnPositions = info.comp.GetPositions(randomSpawnCount);

        // 各座標に対して生成処理を実行
        foreach (var pos in spawnPositions)
        {
            SpawnIndividualUnitAsync(info, pos, ct).Forget();
        }
    }

    /// <summary>
    /// 敵個体ごとのの生成予告、生成フロー処理
    /// </summary>
    /// <param name="info"></param>
    /// <param name="position"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async UniTaskVoid SpawnIndividualUnitAsync(UnitSpawnInfo info, Vector3 position, CancellationToken ct)
    {
        // 予告エフェクトの再生（生成情報側を優先）
        string preview = !string.IsNullOrEmpty(info.previewEffectName) ? info.previewEffectName : _defaultPreviewEffect;
        if (!string.IsNullOrEmpty(preview) && EffectManager.instance != null)
        {
            EffectManager.instance.Play(preview, position);
        }
        
        // SE再生
        if (info.FirstProcessSEName != null && SoundManager.instance != null)
        {
            SoundManager.instance.PlaySE(info.FirstProcessSEName, info.FirstProcessSEVolume);
        }

        // 指定時間待機
        if (info.spawnDelay > 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(info.spawnDelay), cancellationToken: ct);
        }

        // ユニットの生成と初期化
        UnitBase unit = Instantiate(info.unitBase, position, Quaternion.identity);

        // 出現エフェクト
        if (!string.IsNullOrEmpty(info.spawneEffectName) && EffectManager.instance != null)
        {
            EffectManager.instance.Play(info.spawneEffectName, position);
        }

        // チュートリアル用の初期化処理の実行
        SetupTutorialUnit(unit, info);
    }

    /// <summary>
    /// チュートリアル用のユニットの初期化処理
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="info"></param>
    private void SetupTutorialUnit(UnitBase unit, UnitSpawnInfo info)
    {
        // 経験値設定
        unit.DropExp = info.unitBase.UnitStatusData.baseExp * (1 + info.waveIncreaseRate);

        // 管理リストに追加
        _spawnedUnits.Add(unit);

        // 死亡時イベントの購読
        unit.OnUnitDeath += (u) =>
        {
            _spawnedUnits.Remove(u);
            OnEnemyDefeated?.Invoke(u);

            // 生成が全て終わり、かつリストが空になったら全滅通知
            if (!_isSpawning && _spawnedUnits.Count == 0)
            {
                OnAllEnemyDead?.Invoke();
            }
        };

        // HPバーの生成
        if (UnitManager.instance != null) 
            UnitManager.instance.CreateHPBar(unit, _showHPBar);

        // 時限消去
        if (info.destroyTime > 0)
            unit.SetLazyDeath(info.destroyTime);
        
        // クリア対象フラグ
        unit.IsClearTarget = info.isClearTarget;
    }

    // 生成停止
    public void StopSpawning()
    {
        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
        _spawnCts = null;
        _isSpawning = false;
    }

    // 生成済みの敵を削除
    public void DestroyAllEnemy()
    {
        foreach (var unit in _spawnedUnits)
        {
            if (unit != null) unit.SetLazyDeath(0);
        }
        _spawnedUnits.Clear();
    }

    private void OnDestroy()
    {
        StopSpawning();
    }
}

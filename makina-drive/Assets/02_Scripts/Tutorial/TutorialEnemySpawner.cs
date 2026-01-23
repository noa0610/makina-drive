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
        for (int i = 0; i < info.spawnCount; i++)
        {
            if (ct.IsCancellationRequested) break;

            SpawnGroup(info, target);

            if (i < info.spawnCount - 1)
            {
                await UniTask.Delay((int)(info.spawnInterval * 1000), cancellationToken: ct);
            }
        }
        _isSpawning = false; // 生成終了
    }

    private void SpawnGroup(UnitSpawnInfo info, GameObject target)
    {
        List<UnitBase> groupList = new List<UnitBase>();

        for (int j = 0; j < info.sameTimeSpawnCount; j++)
        {
            UnitBase unit = Instantiate(info.unitBase);

            unit.DropExp = info.unitBase.UnitStatusData.baseExp * (1 + info.statusRate);
            unit.IsClearTarget = info.isClearTarget;

            if (info.destroyTime > 0) unit.SetLazyDeath(info.destroyTime);

            groupList.Add(unit);
            _spawnedUnits.Add(unit);

            // 生成した敵の死亡時にリストから除外する処理
            unit.OnUnitDeath += (u) =>
            {
                _spawnedUnits.Remove(u);
                OnEnemyDefeated?.Invoke(u);
                // 生成が終了済みで,リストが空なら全滅
                if (!_isSpawning && _spawnedUnits.Count == 0)
                {
                   OnAllEnemyDead?.Invoke();
                }
            };

            // HPバーを生成
            if (UnitManager.instance != null) UnitManager.instance.CreateHPBar(unit, _showHPBar);

        }

        info.comp.target = target;
        info.comp.Execute(groupList);
    }

    // 生成停止
    public void StopSpawning()
    {
        _spawnCts?.Cancel();
        _spawnCts?.Dispose();
        _spawnCts = null;
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

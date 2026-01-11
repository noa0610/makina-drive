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

    // 生成開始
    public void StartSpawn(UnitSpawnInfo info, GameObject target)
    {
        StopSpawning();
        _spawnCts = new CancellationTokenSource();

        if (info.unitBase == null) return;
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

            // 死亡時にリストから除外する処理
            unit.OnUnitDeath += (u) => _spawnedUnits.Remove(u);
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

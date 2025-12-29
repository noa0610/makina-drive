using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 強化の適用クラス
/// </summary>
public class EnhanceApplier
{
    private StatusManager _statusManager;
    private EnhanceInventory _inventory;

    public EnhanceApplier(StatusManager statusManager, EnhanceInventory inventory)
    {
        _statusManager = statusManager;
        _inventory = inventory;
    }

    public void Apply(EnhanceData data)
    {
        _inventory.Add(data);

        switch(data.type)
        {
            case EnhanceType.statusConstantUp:
                ApplyStatusConstant(data);
                break;
            case EnhanceType.StatusRatioUp:
                ApplyStatusRatio(data);
                break;
            case EnhanceType.Heal:
                ApplyHeal(data);
                break;
        }
    }

    // 固定値で上昇
    private void ApplyStatusConstant(EnhanceData data)
    {
        if(_statusManager.TryGetStatus(data.targetStatus, out var status))
        {
            status.TemporaryChanged += data.ratioPerLevel;
            Debug.Log($"ApplyStatus {data.targetStatus} : {_statusManager.ReadValue(data.targetStatus)}");
        }
    }

    // レベルに合わせて上昇
    private void ApplyStatusRatio(EnhanceData data)
    {
        var level = _inventory.GetLevel(data);

        if(_statusManager.TryGetStatus(data.targetStatus, out var status))
        {
            // 例:２回取得 → 1 + 0.1 * 2 = 1.2
            status.TemporaryChanged = 1f + data.ratioPerLevel * level;
            Debug.Log($"ApplyStatus {data.targetStatus} : {_statusManager.ReadValue(data.targetStatus)}");
        }
    }

    // 体力を回復
    private void ApplyHeal(EnhanceData data)
    {
        var maxHp = _statusManager.ReadValue(Status.MaxHP);
        _statusManager.TakeHeal(maxHp * data.healRatio);
    }
}

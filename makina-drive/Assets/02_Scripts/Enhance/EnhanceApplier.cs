using UnityEngine;
using UnityEngine.EventSystems;

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
            case EnhanceType.StatusRatioUp:
                ApplyStatusRatio(data);
                break;
            case EnhanceType.Heal:
                ApplyHeal(data);
                break;
        }
    }

    private void ApplyStatusRatio(EnhanceData data)
    {
        var level = _inventory.GetLevel(data);

        if(_statusManager.TryGetStatus(data.targetStatus, out var status))
        {
            // 例:２回取得 → 1 + 0.1 * 2 = 1.2
            status.TemporaryChanged = 1f + data.ratioPerLevel * level;
        }
    }

    private void ApplyHeal(EnhanceData data)
    {
        var maxHp = _statusManager.ReadValue(Status.MaxHP);
        _statusManager.TakeHeal(maxHp * data.healRatio);
    }
}

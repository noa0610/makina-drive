using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

/// <summary>
/// ユニットが持つステータスを一元管理
/// </summary>
public class StatusManager
{
    private Dictionary<Status, StatusInfo> _statusAmounts = new();
    private UnitTags unitTags;
    public StatusManager Initialize(UnitStatusData data)
    {
        _statusAmounts.Clear();

        // 基礎ステータスを一括登録
        var hp = AddStatus(Status.HP, data.hp, false, true);
        var mHp = AddStatus(Status.MaxHP, data.maxHp, true, true);
        mHp.OnAmountChanged += (before, after) =>
        {
            hp.SetMax(mHp.CurrentAmount);

            if(after > before)
            {
                float diff = after - before;
                hp.CurrentAmount += diff;
                Debug.Log($"MaxHP Increased: {before} -> {after}. Added {diff} to Current HP.");
            }
        };
        AddStatus(Status.DamageRatio, data.damageTakeScale);
        AddStatus(Status.Speed, data.speed);
        AddStatus(Status.DashSpeed, data.dashSpeed);
        AddStatus(Status.ATK, data.atk);
        AddStatus(Status.DEF, data.def);
        AddStatus(Status.CollectionRange, data.collectionRange);
        AddStatus(Status.Stamina, data.stamina, false);
        AddStatus(Status.knockbackMultiplier, data.knockbackMultiplier);
        AddStatus(Status.knockbackResistance, data.knockbackResistance);
        AddStatus(Status.Lv, 1);
        unitTags = data.tags;
        return this;
    }

    public StatusInfo AddStatus(Status status, float amount, bool isDynamic = true, bool isHideIfDefault = false)
    {
        var info = new StatusInfo(amount, isDynamic);
        info.SetMin(0);
        _statusAmounts[status] = info;
        return info;
    }
    public bool RemoveStatus(Status status)
    {
        return _statusAmounts.Remove(status);
    }
    public IEnumerable<StatusInfo> GetAllInfos()
    {
        foreach (var m in _statusAmounts.Values)
            yield return m;
    }

    public bool IsRegistered(Status status) => _statusAmounts.ContainsKey(status);

    public bool TryGetStatus(Status status, out StatusInfo model)
    {
        return _statusAmounts.TryGetValue(status, out model);
    }
    public bool TryGetCurrentValue(Status status, out float current)
    {
        current = float.MinValue;
        if (_statusAmounts.TryGetValue(status, out var model))
        {
            current = model.CurrentAmount;
            return true;
        }
        return false;
    }
    public StatusInfo GetStatus(Status status)
    {
        return _statusAmounts[status];
    }

    public void AddValue(Status status, float value)
    {
        if (TryGetStatus(status, out var info))
        {
            info.CurrentAmount += value; 
            Debug.Log($"Status {status} changed by {value}. New value: {info.CurrentAmount}");
        }
        else
        {
            Debug.LogWarning($"Status {status} is not registered. Cannot add value.");
        }
    }

    public float ReadValue(Status status)
    {
        if (TryGetStatus(status, out var info))
        {
            return info.CurrentAmount;
        }
        return float.NaN;
    }
    public bool TryReadValue(Status status, out float value)
    {
        value = ReadValue(status);
        return !float.IsNaN(value);
    }
    public void TakeHeal(float value)
    {
        _statusAmounts[Status.HP].CurrentAmount += value;
    }

    /// <returns>死亡したかどうか</returns>
    public bool TakeDamage(float value)
    {
        var s = _statusAmounts[Status.HP];
        s.CurrentAmount -= value;
        // Debug.Log($"TakeDamage: {value}, HP: {s.CurrentAmount}/{_statusAmounts[Status.MaxHP].CurrentAmount}");
        return s.CurrentAmount <= 0;
    }

    public UnitTags ReadUnitTag()
    {
        return unitTags;
    }

    // 特定のステータスを一括強化適用
    public void ApplyStatusMultiplier(List<Status> statuses, float multiplier)
    {
        foreach(var type in statuses)
        {
            if(TryGetStatus(type, out var info))
            {
                // TemporaryChangedは1.0がデフォルトなので、そこに加算する
                info.TemporaryChanged = 1f + multiplier;
            }
        }
    }
}

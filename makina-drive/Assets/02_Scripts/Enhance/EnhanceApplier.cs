using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 強化の適用クラス
/// </summary>
public class EnhanceApplier
{
    private StatusManager _statusManager;
    private EnhanceInventory _inventory;
    private RecoveryStatus _recoveryStatus;

    public EnhanceApplier(StatusManager statusManager, EnhanceInventory inventory, RecoveryStatus recoveryStatus)
    {
        _statusManager = statusManager;
        _inventory = inventory;
        _recoveryStatus = recoveryStatus;
    }

    public void Apply(EnhanceData data)
    {
        _inventory.Add(data);

        switch (data.type)
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
            case EnhanceType.RecoveryRateUp:
                ApplyRecoveryRate(data);
                break;
            case EnhanceType.RecoveryMultiplierUp:
                ApplyRecoveryMultiplier(data);
                break;
        }
    }

    // 固定値で上昇
    private void ApplyStatusConstant(EnhanceData data)
    {
        if (_statusManager.TryGetStatus(data.targetStatus, out var status))
        {
            status.TemporaryChanged += data.ratioPerLevel;
            Debug.Log($"ApplyStatus {data.targetStatus} : {_statusManager.ReadValue(data.targetStatus)}");
        }
    }

    // レベルに合わせて上昇
    private void ApplyStatusRatio(EnhanceData data)
    {
        var level = _inventory.GetLevel(data);

        if (_statusManager.TryGetStatus(data.targetStatus, out var status))
        {
            // 例:２回取得 → 1 + 0.1 * 2 = 1.2 (上昇値120%)
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

    // 自動回復のベース値を増加
    private void ApplyRecoveryRate(EnhanceData data)
    {
        if (!_statusManager.TryGetStatus(data.targetStatus, out var info)) return;

        // 初回登録
        if (!_recoveryStatus.IsRegistered(data.targetStatus))
        {
            Func<float> maxValueGetter = data.targetStatus switch
            {
                Status.HP => () => _statusManager.ReadValue(Status.MaxHP),
                Status.Stamina => () => _statusManager.ReadValue(Status.MaxStamina),
                _ => () => float.MaxValue
            };

            _recoveryStatus.SetRecovery(
                data.targetStatus,
                baseRate: data.ratioPerLevel,
                percentRate: 0,
                multiplier: 1.0f,
                delay: 1.0f,
                penaltyDelay: 0,
                isPulse: true,
                maxValueGetter: maxValueGetter
            );
            Debug.Log($"Initial Recovery Registered: {data.targetStatus}");
        }
        // 2回目以降
        else
        {
            // 既存の回復量に加算
            _recoveryStatus.AddBaseRate(data.targetStatus, data.ratioPerLevel);
            Debug.Log($"Recovery Rate Up: {data.targetStatus} +{data.ratioPerLevel}");
        }
    }

    // 回復効率を増加
    private void ApplyRecoveryMultiplier(EnhanceData data)
    {
        if (!_statusManager.TryGetStatus(data.targetStatus, out var info)) return;

        // 例: 2回取得 → 1.0 + 0.1 * 2 = 1.2 (回復速度120%)
        float totalMultiplier = 1f + (data.ratioPerLevel * _inventory.GetLevel(data));

        // 初回登録
        if (!_recoveryStatus.IsRegistered(data.targetStatus))
        {
            InitializeRecovery(data.targetStatus);
        }

        _recoveryStatus.UpdateMultiplier(data.targetStatus, totalMultiplier);
    }

    private void InitializeRecovery(Status targetStatus)
    {
        Func<float> maxValueGetter = targetStatus switch
        {
            Status.HP => () => _statusManager.ReadValue(Status.MaxHP),
            Status.Stamina => () => _statusManager.ReadValue(Status.MaxStamina),
            _ => () => float.MaxValue
        };

        _recoveryStatus.SetRecovery(
            targetStatus,
            baseRate: 0,
            percentRate: 0,
            multiplier: 1.0f,
            delay: 0.5f,
            penaltyDelay: 1.0f,
            isPulse: false,
            maxValueGetter: maxValueGetter
        );
    }
}

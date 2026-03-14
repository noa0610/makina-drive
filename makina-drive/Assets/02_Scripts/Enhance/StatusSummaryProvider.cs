using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステータスを計算してUI用クラスに変換する
/// </summary>
public class StatusSummaryProvider
{
    private readonly StatusManager _statusManager;
    private readonly EnhanceInventory _inventory;

    public StatusSummaryProvider(StatusManager statusManager, EnhanceInventory inventory)
    {
        _statusManager = statusManager;
        _inventory = inventory;
    }

    public List<StatusSummaryViewModel> GetSummary(List<StatusDisplayConfiguration> configs)
    {
        var viewModels = new List<StatusSummaryViewModel>();
        if (configs == null || configs.Count == 0) return viewModels;

        foreach (var setting in configs[0].settings)
        {
            float diff = 0;
            if (setting.displaySource == DisplaySource.StatusManager)
            {
                if (!_statusManager.TryGetStatus(setting.targetStatus, out var info)) continue;


                // 現在の最終的な値を取得
                float currentValue = _statusManager.ReadValue(setting.targetStatus);
                float baseValue = info.DefaultAmount;
                diff = currentValue - baseValue;
            }
            else
            {
                // 特定のステータスに対する関連データの ratioPerLevel×取得数 を合計する
                diff = CalculateAccumulatedEnhance(setting.targetStatus);
            }

            // 変化がほぼ0ならNone
            StatusChangeState state = StatusChangeState.None;
            if (diff > 0.0001f) state = StatusChangeState.Positive;
            else if (diff < -0.0001f) state = StatusChangeState.Negative;

            viewModels.Add(new StatusSummaryViewModel
            {
                icon = setting.icon,
                name = setting.displayName,
                diffValueText = (diff > 0 ? "+" : "") + FormatValue(diff, setting.type),
                changeState = state,
                isInvertedBenefit = setting.isInvertedBenefit,
            });
        }
        return viewModels;
    }

    // 特定のステータスに紐づく強化の累積値を計算
    private float CalculateAccumulatedEnhance(Status status)
    {
        float total = 0;

        // 取得済みの強化から対象のステータスを操作する者を抽出
        foreach (var data in _inventory.GetAllAcquiredData())
        {
            if (data.targetStatus == status)
            {
                if (data.type == EnhanceType.RecoveryRateUp ||
                    data.type == EnhanceType.RecoveryMultiplierUp)
                {
                    total += data.ratioPerLevel * _inventory.GetLevel(data);
                }
            }
        }
        return total;
    }

    private string FormatValue(float val, DisplayValueType type)
    {
        return type == DisplayValueType.Percent ? $"{(val * 100):F0}%" : val.ToString("F1");
    }
}

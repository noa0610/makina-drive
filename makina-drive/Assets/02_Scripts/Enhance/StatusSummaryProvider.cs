using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステータスを計算してUI用クラスに変換する
/// </summary>
public class StatusSummaryProvider
{
    private readonly StatusManager _statusManager;

    public StatusSummaryProvider(StatusManager statusManager)
    {
        _statusManager = statusManager;
    }

    public List<StatusSummaryViewModel> GetSummary(List<StatusDisplayConfiguration> configs)
    {
        var viewModels = new List<StatusSummaryViewModel>();
        if (configs == null || configs.Count == 0) return viewModels;

        foreach (var setting in configs[0].settings)
        {
            if (!_statusManager.TryGetStatus(setting.targetStatus, out var info)) continue;

            // 現在の最終的な値を取得
            float currentValue = _statusManager.ReadValue(setting.targetStatus);
            float baseValue = info.DefaultAmount;
            float diff = currentValue - baseValue;

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

    private string FormatValue(float val, DisplayValueType type)
    {
        return type == DisplayValueType.Percent ? $"{(val * 100):F0}%" : val.ToString("F1");
    }
}

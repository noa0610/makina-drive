using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 登録したステータスを自動で回復（加算）する
/// </summary>
public class RecoveryStatus
{
    // 回復設定保持クラス
    private class RecoverySettings
    {
        public StatusInfo Info;
        public Func<float> MaxValueGetter; // 回復上限値を動的に取得
        public float BaseRate;
        public float Multiplier;
        public float Delay;
        public float Timer;
        public bool IsLocked;

        public float GetTotalRate()
        {
            float max = MaxValueGetter?.Invoke() ?? Info.DefaultAmount;
            return BaseRate * Multiplier;
        }
    }

    private readonly StatusManager _statusManager;
    private readonly Dictionary<Status, RecoverySettings> _recoveryMap = new();
    private bool GlovalLocked { get; set; } // 一括停止用


    public RecoveryStatus(StatusManager statusManager)
    {
        _statusManager = statusManager;
    }

    /// <summary>
    /// 自動回復ステータスを登録または更新
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void SetRecovery(Status status, float BaseRate, float multiplier, float delay)
    {
        // 既存設定の更新
        if (_recoveryMap.TryGetValue(status, out var settings))
        {
            settings.BaseRate = BaseRate;
            settings.Multiplier = multiplier;
            settings.Delay = delay;
            return;
        }

        // 新規登録
        if (_statusManager.TryGetStatus(status, out var info))
        {
            var newSettings = new RecoverySettings
            {
                BaseRate = BaseRate,
                Multiplier = multiplier,
                Delay = delay,
                Timer = delay, // 即時回復を開始
                Info = info
            };

            info.OnAmountChanged += (before, after) =>
            {
                if (after < before)
                {
                    newSettings.Timer = 0f;
                }
            };

            _recoveryMap.Add(status, newSettings);
        }
    }

    /// <summary>
    /// 指定したステータスの自動回復を解除
    /// </summary>
    /// <param name="status"></param>
    public void RemoveRecovery(Status status)
    {
        _recoveryMap.Remove(status);
    }

    /// <summary>
    /// すべてのステータスの自動回復を解除
    /// </summary>
    /// <param name="status"></param>
    public void ClearAll()
    {
        _recoveryMap.Clear();
    }

    /// <summary>
    /// 毎フレーム回復処理
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime)
    {
        foreach (var kvp in _recoveryMap)
        {
            var config = kvp.Value;
            var info = config.Info;

            // 最大値に達していたらスキップ
            if (info.CurrentAmount >= info.DefaultAmount) continue;

            // 遅延タイマーの更新
            if (config.Timer < config.Delay)
            {
                config.Timer += deltaTime;
                continue;
            }

            float recoverAmount = config.GetTotalRate() + deltaTime;
            info.CurrentAmount += recoverAmount;
        }
    }
}

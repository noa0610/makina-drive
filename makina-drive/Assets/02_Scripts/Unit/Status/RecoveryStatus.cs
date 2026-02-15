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
        public float BaseRate;             // 固定値（毎秒）
        public float PercentRate;          // 割合（0.01 = 1%）
        public float Multiplier;           // 倍率

        public float Delay;                // 回復待機時間
        private bool _isPenaltyActive;     // 値が0になった時のペナルティ
        public float PenaltyDelay;         // ペナルティの追加待機時間
        public float Timer;                // 経過時間
        public bool IsLocked;              // 回復停止フラグ

        public bool IsPulse;
        public float PulseInterval;
        private float _pulseTimer;

        public RecoverySettings(StatusInfo info)
        {
            Info = info;
            // 減少時の処理
            Info.OnAmountChanged += (before, after) =>
            {
                if (after < before)
                {
                    Timer = 0f; // ダメージ等でタイマーリセット

                    // 値が0（または最小値）になったらペナルティ発動
                    if (after <= 0) _isPenaltyActive = true;
                }

                // 最大まで回復したらペナルティ解除
                float max = MaxValueGetter?.Invoke() ?? Info.DefaultAmount;
                if (after >= max) _isPenaltyActive = false;
            };
        }

        public float GetTotalRate()
        {
            float max = MaxValueGetter?.Invoke() ?? Info.DefaultAmount;
            return BaseRate * Multiplier;
        }
        public float GetTotalDelay() => _isPenaltyActive ? (Delay + PenaltyDelay) : Delay;

        public float CalculateTickAmount(float deltaTime, float currentMax)
        {
            float ratePerSecond = (BaseRate + (currentMax * PercentRate)) * Multiplier;
            return ratePerSecond * deltaTime;
        }

        public void UpdatePulse(float deltaTime, float currentMax, Status status, Action<Status, float> onRecovered)
        {
            _pulseTimer += deltaTime;
            if (_pulseTimer >= PulseInterval)
            {
                // インターバル分の回復量を計算 (毎秒の回復量 * インターバル秒)
                float amount = CalculateTickAmount(PulseInterval, currentMax);
                ApplyRecovery(amount, currentMax, status, onRecovered);
                _pulseTimer = 0f;
            }
        }

        public void ApplyRecovery(float amount, float currentMax, Status status, Action<Status, float> onRecovered)
        {
            if (amount <= 0) return;

            float before = Info.CurrentAmount;
            Info.CurrentAmount += amount;
            float actualRecovered = Info.CurrentAmount - before;

            if (actualRecovered > 0)
            {
                onRecovered?.Invoke(status, actualRecovered);
            }
        }
    }

    private readonly StatusManager _statusManager;
    private readonly Dictionary<Status, RecoverySettings> _recoveryMap = new();
    private bool GlobalLocked { get; set; } // 一括停止用

    public event Action<Status, float> OnRecovered; // 回復イベント


    public RecoveryStatus(StatusManager statusManager)
    {
        _statusManager = statusManager;
    }

    /// <summary>
    /// 自動回復ステータスを登録または更新
    /// </summary>
    /// <param name="before"></param>
    /// <param name="after"></param>
    public void SetRecovery(Status status, float baseRate, float percentRate, float multiplier, float delay,
                            float penaltyDelay = 0f, bool isPulse = false, float pulseInterval = 1f,
                            Func<float> maxValueGetter = null)
    {
        if (!_statusManager.TryGetStatus(status, out var info)) return;

        // 既存設定の更新
        if (!_recoveryMap.TryGetValue(status, out var settings))
        {
            settings = new RecoverySettings(info);
            _recoveryMap.Add(status, settings);
        }

        settings.BaseRate = baseRate;
        settings.PercentRate = percentRate;
        settings.Multiplier = multiplier;
        settings.Delay = delay;
        settings.PenaltyDelay = penaltyDelay;
        settings.IsPulse = isPulse;
        settings.PulseInterval = pulseInterval;
        settings.MaxValueGetter = maxValueGetter ?? (() => info.DefaultAmount);
        settings.Timer = settings.GetTotalDelay(); // 登録時は即時回復可能に
    }

    public bool IsRegistered(Status status)
    {
        return _recoveryMap.ContainsKey(status);
    }

    public void UpdateMultiplier(Status status, float multiplier)
    {
        if (_recoveryMap.TryGetValue(status, out var s)) s.Multiplier = multiplier;
    }

    public void AddBaseRate(Status status, float addValue)
    {
        if (_recoveryMap.TryGetValue(status, out var s)) s.BaseRate += addValue;
    }

    public void SetLock(Status status, bool isLocked)
    {
        if (_recoveryMap.TryGetValue(status, out var s)) s.IsLocked = isLocked;
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
        if (GlobalLocked) return;

        foreach (var kvp in _recoveryMap)
        {
            var status = kvp.Key;
            var settings = kvp.Value;

            if (settings.IsLocked) continue;

            float currentMax = settings.MaxValueGetter.Invoke();
            if (settings.Info.CurrentAmount >= currentMax) continue;

            // ディレイ計測（ペナルティ加算分を含む）
            if (settings.Timer < settings.GetTotalDelay())
            {
                settings.Timer += deltaTime;
                continue;
            }

            // 回復ロジックの切り替え
            if (settings.IsPulse)
            {
                settings.UpdatePulse(deltaTime, currentMax, status, OnRecovered);
            }
            else
            {
                float amount = settings.CalculateTickAmount(deltaTime, currentMax);
                settings.ApplyRecovery(amount, currentMax, status, OnRecovered);
            }
        }
    }
}

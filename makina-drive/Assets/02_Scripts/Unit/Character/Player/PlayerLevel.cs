using System;
using UnityEngine;

/// <summary>
/// プレイヤーのレベル関連の計算クラス
/// </summary>
[Serializable]
public class PlayerLevel
{
    [SerializeField] private float _farstNextLevelExp = 10;
    [SerializeField] private float _nextLevelExpRate = 1.2f;
    [SerializeField] private int _maxLevel = 100;
    private UnitBase _player;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public int EnhancementPoints { get; private set; } = 0; // 強化権のストック（レベルアップで加算）
    public float ExpToNextLevel;


    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // 現在地、最大値
    public event Action<int> OnEnhancementPointsChanged;

    public PlayerLevel(UnitBase player, float farstNextExp = 10f, float exptoNextLevelRate = 1.2f)
    {
        _player = player;
        _farstNextLevelExp = farstNextExp;
        _nextLevelExpRate = exptoNextLevelRate;
        ExpToNextLevel = _farstNextLevelExp;
        Debug.Log(ExpToNextLevel);
    }

    public void AddExp(float amount)
    {
        if (amount <= 0 && CurrentLevel >= _maxLevel) return;

        
        Debug.Log($"farst NextLevelExp : {_farstNextLevelExp}");
        Debug.Log($"NextLevelExpRate : {_nextLevelExpRate}");

        // 安全のためのカウンター（無限ループ防止)
        int safetyCounter = 0;
        const int maxLevelsPerFrame = 100;

        CurrentExp += amount;
        Debug.Log($"Get {amount} Exp.  currentExp {CurrentExp}");

        while (CurrentExp >= ExpToNextLevel && safetyCounter < maxLevelsPerFrame)
        {
            LevelUp();
            safetyCounter++;
        }

        if (safetyCounter >= maxLevelsPerFrame)
        {
            Debug.LogWarning("一回の経験値獲得でレベルアップ上限に達しました。計算式を確認してください。");
        }


        OnExpChanged?.Invoke(CurrentExp, ExpToNextLevel);
    }

    private void LevelUp()
    {
        CurrentExp -= ExpToNextLevel;
        CurrentLevel++;

        // 強化権のストック
        EnhancementPoints++;
        OnEnhancementPointsChanged?.Invoke(EnhancementPoints);

        NextLevelCalculations();
        OnLevelUp?.Invoke(CurrentLevel);

        Debug.Log($"Level Up!  Current Level : {CurrentLevel}");
    }

    // 次のレベルの計算
    private void NextLevelCalculations()
    {
        ExpToNextLevel = ExpToNextLevel * _nextLevelExpRate;
        Debug.Log($"Next Level Exp : {ExpToNextLevel}");
    }

    // ステータスの強化
    public void ApplyUpgrade(Status status, float value)
    {
        if (_nextLevelExpRate <= 1.0f) _nextLevelExpRate = 1.1f;

        _player.statusManager.AddValue(status, value);
    }

    // 強化権を消費しイベント発火
    public void ConsumeEnhancementPoint()
    {
        if(EnhancementPoints > 0)
        {
            EnhancementPoints--;
            OnEnhancementPointsChanged?.Invoke(EnhancementPoints);
        }
    }
}

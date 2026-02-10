using System;
using UnityEngine;

/// <summary>
/// プレイヤーのレベル関連の計算クラス
/// </summary>
[Serializable]
public class PlayerLevel
{
    [SerializeField] private float _baseExp = 20;
    [SerializeField] private float _linearWeight = 8f;      // レベルのに比例して増える分
    [SerializeField] private float _quadraticWeight = 2f;   // レベルの2乗で増える分
    [SerializeField] private int _maxLevel = 100;
    private UnitBase _player;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public int EnhancementPoints { get; private set; } = 0; // 強化権のストック（レベルアップで加算）
    public float ExpToNextLevel;


    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // 現在地、最大値
    public event Action<int> OnEnhancementPointsChanged;

    public PlayerLevel(UnitBase player, float baseExp = 20f, float linearWeight = 8f, float quadraticWeight = 2f)
    {
        _player = player;
        _baseExp = baseExp;
        _linearWeight = linearWeight;
        _quadraticWeight = quadraticWeight;
        NextLevelCalculations();
        Debug.Log(ExpToNextLevel);
    }

    public void AddExp(float amount)
    {
        if (amount <= 0 && CurrentLevel >= _maxLevel) return;

        
        Debug.Log($"BaseExp : {_baseExp}");

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
        ExpToNextLevel = _baseExp + 
                     (_linearWeight * CurrentLevel) + 
                     (_quadraticWeight * Mathf.Pow(CurrentLevel, 2));
        Debug.Log($"Next Level Exp : {ExpToNextLevel}");
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

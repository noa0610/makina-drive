using System;
using UnityEngine;

[Serializable]
public class PlayerLevel
{
    private UnitBase _player;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; } = 0;
    public float ExpToNextLevel => CurrentLevel * 10; // TODO 次のレベルまでの計算式

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExpChanged; // 現在地、最大値

    public PlayerLevel(UnitBase player)
    {
        _player = player;
    }

    public void AddExp(float amount)
    {
        CurrentExp += amount;
        
        Debug.Log($"Get {amount} Exp.  currentExp {CurrentExp}");

        while (CurrentExp >= ExpToNextLevel)
        {
            LevelUp();
        }
        OnExpChanged?.Invoke(CurrentExp, ExpToNextLevel);
    }

    private void LevelUp()
    {
        CurrentExp-= ExpToNextLevel;
        CurrentLevel++;

        OnLevelUp?.Invoke(CurrentLevel);

        Debug.Log($"Level Up!  Current Level : {CurrentLevel}");
    }

    public void ApplyUpgrade(Status status, float value)
    {
        _player.statusManager.AddValue(status, value);
    }
}

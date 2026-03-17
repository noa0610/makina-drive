using System;
using UnityEngine;

/// <summary>
/// スコア管理
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private string _stageId; // ステージ識別用

    private int _currentScore = 0;
    public int CurrentScore => _currentScore;

    public event Action<int> OnScoreChanged;

    private void Start()
    {
        UnitBase.OnEnemyDefeated += AddScore;
    }

    // スコア加算
    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        _currentScore += amount;
        OnScoreChanged?.Invoke(_currentScore);
    }

    // ゲームクリア時の処理
    public void SaveHighScore()
    {
        HighScoreSaveDataManager.instance.UpdateHighScore(_stageId, _currentScore);
    }

    // ハイスコア取得
    public int GetHighScore()
    {
        return HighScoreSaveDataManager.instance.GetHighScore(_stageId);
    }

    public void OnDestroy()
    {
        UnitBase.OnEnemyDefeated -= AddScore;
    }
}

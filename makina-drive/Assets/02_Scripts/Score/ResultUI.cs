using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// リザルトスコア表示
/// </summary>
public class ResultUI : MonoBehaviour
{
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private GameState _displayGameState;
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;
    [SerializeField] private float _delayTime = 1f;

    private void Start()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        _resultPanel.SetActive(false);
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == _displayGameState)
        {
            ShowResult();
        }
    }

    private async void ShowResult()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(_delayTime));
        _scoreManager.SaveHighScore();
        _resultPanel.SetActive(true);
        _finalScoreText.text = $"Final Score: {_scoreManager.CurrentScore}";
        _highScoreText.text = $"High Score: {_scoreManager.GetHighScore()}";
    }
}

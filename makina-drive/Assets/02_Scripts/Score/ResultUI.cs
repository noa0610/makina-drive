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
    [SerializeField] private TextMeshProUGUI _finalScoreText;
    [SerializeField] private TextMeshProUGUI _highScoreText;

    private void Start()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        _resultPanel.SetActive(false);
    }

    private void HandleStateChanged(GameState state)
    {
        if (state == GameState.Clear)
        {
            ShowResult();
        }
    }

    private void ShowResult()
    {
        _scoreManager.SaveHighScore();

        _resultPanel.SetActive(true);
        _finalScoreText.text = $"Final Score: {_scoreManager.CurrentScore}";
        _highScoreText.text = $"High Score: {_scoreManager.GetHighScore()}";
    }
}

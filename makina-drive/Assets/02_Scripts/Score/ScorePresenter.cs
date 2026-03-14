using TMPro;
using UnityEngine;


/// <summary>
/// スコア表示UI
/// </summary>
public class ScorePresenter : MonoBehaviour
{
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private string _prefix;

    private void Start()
    {
        _scoreManager.OnScoreChanged += UpdateScoreText;
        UpdateScoreText(_scoreManager.CurrentScore);
    }

    private void Oestroy()
    {
        _scoreManager.OnScoreChanged -= UpdateScoreText;
    }

    private void UpdateScoreText(int score)
    {
        _scoreText.text = $"{_prefix}{score:N0}";
    }
}

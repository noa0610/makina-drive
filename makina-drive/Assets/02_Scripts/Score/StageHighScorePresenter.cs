using TMPro;
using UnityEngine;

public class StageHighScorePresenter : MonoBehaviour
{
    [SerializeField] private string _targetStageId;
    [SerializeField] private TextMeshProUGUI _displayScoreText;
    [SerializeField] private string _prefix = "ベストスコア：";

    private void Start()
    {
        RefreshDisplay();
    }

    // ステージ毎のハイスコアを表示
    public void RefreshDisplay()
    {
        if (HighScoreSaveDataManager.instance != null)
        {
            int highScore = HighScoreSaveDataManager.instance.GetHighScore(_targetStageId);
            _displayScoreText.text = $"{_prefix}{highScore:N0}";
        }
        else
        {
            _displayScoreText.text = $"{_prefix}0";
        }
    }
}

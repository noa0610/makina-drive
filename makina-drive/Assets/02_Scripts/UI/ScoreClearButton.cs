using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScoreClearButton : MonoBehaviour
{
    public enum ClearMode { SpecificStage, AllStages }

    [Header("Settings")]
    [SerializeField] private ClearMode _clearMode;
    [SerializeField] private string _targetStageId; // インスペクターで指定

    [Header("UI References")]
    [SerializeField] private GameObject _confirmationPanel; // 「本当に削除しますか？」パネル
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [SerializeField] private VisualInfo _ButtonSE;
    [SerializeField] private VisualInfo _YesButtonSE;
    [SerializeField] private VisualInfo _NoButtonSE;

    private void Start()
    {
        // 初期状態では確認パネルを非表示に
        if (_confirmationPanel != null) _confirmationPanel.SetActive(false);

        // ボタンのクリックイベント登録
        GetComponent<Button>().onClick.AddListener(ShowConfirmation);
        _yesButton.onClick.AddListener(ExecuteDelete);
        _noButton.onClick.AddListener(HideConfirmation);
    }

    private void ShowConfirmation()
    {
        _confirmationPanel.SetActive(true);
        if(SoundManager.instance != null && _ButtonSE.SEName  != null)
        {
            SoundManager.instance.PlaySE(_ButtonSE.SEName, _ButtonSE.Volume);
        }
    }

    private void HideConfirmation()
    {
        _confirmationPanel.SetActive(false);
        if(SoundManager.instance != null && _NoButtonSE.SEName  != null)
        {
            SoundManager.instance.PlaySE(_NoButtonSE.SEName, _NoButtonSE.Volume);
        }
    }

    private void ExecuteDelete()
    {
        // データの削除実行
        if (_clearMode == ClearMode.AllStages)
        {
            HighScoreSaveDataManager.instance.DeleteAllHighScores();
        }
        else
        {
            HighScoreSaveDataManager.instance.DeleteHighScore(_targetStageId);
        }

        if(SoundManager.instance != null && _YesButtonSE.SEName  != null)
        {
            SoundManager.instance.PlaySE(_YesButtonSE.SEName, _YesButtonSE.Volume);
        }

        _confirmationPanel.SetActive(false);
    }
}

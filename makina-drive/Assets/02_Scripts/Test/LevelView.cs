using TMPro;
using UnityEngine;


public class LevelView : MonoBehaviour
{
    [SerializeField] Freya _target;
    [SerializeField] TextMeshProUGUI _text;
    private PlayerLevel _level;

    private void Start()
    {
        // Startで購読することで、Freya側の初期化完了を待つ
        TrySubscribe();
    }


    // // 状態変更イベントを購読
    // private void OnEnable()
    // {
    //     _level = _target._level;

    //     _level.OnLevelUp += HandleLevelUp;
    //     Debug.Log("OnEnable");
    // }

    private void TrySubscribe()
    {
        if (_target == null) return;

        // すでに登録済みの場合はスキップ
        if (_level != null) return;

        _level = _target._level;

        _level.OnLevelUp += HandleLevelUp;
        Debug.Log("LevelUpイベントの購読に成功しました");

    }

    // 購読解除
    private void OnDisable()
    {
        _level.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int nextLevel)
    {
        UpdateText(nextLevel);
    }

    private void UpdateText(int level)
    {
        Debug.Log($"update text");
        if (_text != null)
        {
            Debug.Log($"update LevelView : {level}");
            _text.text = "Lv. " + level;
        }
    }
}

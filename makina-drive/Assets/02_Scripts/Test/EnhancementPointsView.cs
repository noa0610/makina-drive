using TMPro;
using UnityEngine;

public class EnhancementPointsView : MonoBehaviour
{
    [SerializeField] private Freya _target;
    [SerializeField] private TextMeshProUGUI _text;
    private PlayerLevel _level;

    private void Start()
    {
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (_target == null) return;

        // すでに登録済みの場合はスキップ
        if (_level != null) return;
        
        _level = _target._level;
        _level.OnEnhancementPointsChanged += HandleEnhancementPointsChanged;
    }

    // 購読解除
    private void OnDisable()
    {
        _level.OnEnhancementPointsChanged -= HandleEnhancementPointsChanged;
    }

    private void HandleEnhancementPointsChanged(int points)
    {
        _text.text = $"{points}";
    }

}

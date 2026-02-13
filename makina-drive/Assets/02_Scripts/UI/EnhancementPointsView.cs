using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnhancementPointsView : MonoBehaviour
{
    [SerializeField] private Freya _target;
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _text;
    private PlayerLevel _level;
    private bool _isImmediateEnhancement;

    private void Start()
    {
        TrySubscribe();

    }

    private void TrySubscribe()
    {
        if (_target == null) return;

        // すでに登録済みの場合はスキップ
        if (_level != null) return;

        if (VisualSettingsManager.instance != null && VisualSettingsManager.instance.Settings.isImmediateEnhancement)
        {
            // 即強化の設定がある場合は非表示
            _panel.SetActive(false);
            _isImmediateEnhancement = VisualSettingsManager.instance.Settings.isImmediateEnhancement;
            return;
        }

        _level = _target._level;
        _level.OnEnhancementPointsChanged += HandleEnhancementPointsChanged;
    }

    // 購読解除
    private void OnDisable()
    {
        if (_isImmediateEnhancement) return;

        _level.OnEnhancementPointsChanged -= HandleEnhancementPointsChanged;
    }

    private void HandleEnhancementPointsChanged(int points)
    {
        _text.text = $"{points}";
    }

}

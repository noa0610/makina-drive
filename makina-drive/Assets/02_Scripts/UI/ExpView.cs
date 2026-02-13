using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpView : MonoBehaviour
{
    [SerializeField] Freya _target;
    [SerializeField] Slider _slider;
    private PlayerLevel _level;

    private void Start()
    {
        // Startで購読することで、Freya側の初期化完了を待つ
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (_target == null) return;

        // すでに登録済みの場合はスキップ
        if (_level != null) return;

        _level = _target._level;

        _level.OnExpChanged += HandleExpUp;
    }

    // 購読解除
    private void OnDisable()
    {
        _level.OnExpChanged -= HandleExpUp;
    }

    private void HandleExpUp(float currentExp, float maxExp)
    {
        UpdateSlidar(currentExp, maxExp);
    }

    private void UpdateSlidar(float currentExp, float maxExp)
    {
        if (_slider != null)
        {
            _slider.maxValue = maxExp;
            _slider.value = currentExp;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステータスの値をスライダーで表示
/// </summary>
public class StatusSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private UnitBase _targetUnit;
    [SerializeField] private Status _viewStatus;

    private Status _status => _viewStatus;
    private StatusInfo _maxAmountInfo;
    private StatusInfo _currentAmountInfo;


    private void Start()
    {
        if (_targetUnit)
        {
            Setup(_targetUnit);
        }
    }

    public void Setup(UnitBase target)
    {
        if (target == null)
        {
            Debug.LogError("StatusSlider: Target Unit is not assigned.");
            return;
        }

        // 現在値のStatusInfoを取得し、イベントを購読
        if (target.statusManager.TryGetStatus(_status, out _currentAmountInfo))
        {
            _currentAmountInfo.OnAmountChanged += OnCurrentAmountChanged;
        }
        else
        {
            Debug.LogError($"StatusSlider: {_status} is not registered in StatusManager.");
            return;
        }

        // 最大値のStatusInfoを取得し、イベントを購読（HPの場合、MaxHPが必要）
        if (_status == Status.HP)
        {
            if (target.statusManager.TryGetStatus(Status.MaxHP, out _maxAmountInfo))
            {
                _maxAmountInfo.OnAmountChanged += OnMaxAmountChanged;
            }
        }

        // 初期値の設定
        UpdateSliderValues();

        Debug.Log($"StatusSlider: Initialized for {_status} with Max {_slider.maxValue} and Current {_slider.value}");
    }

    // 現在地の変更イベントハンドラ
    private void OnCurrentAmountChanged(float before, float after)
    {
        _slider.value = after;
        // Debug.Log($"StatusSlider: {_status} updated from {before} to {after}. Slider Value: {_slider.value}");
    }

    // 最大値の変更イベントハンドラ
    private void OnMaxAmountChanged(float before, float after)
    {
        _slider.maxValue = after;
        _slider.value = _currentAmountInfo.CurrentAmount;
        // Debug.Log($"StatusSlider: MaxHP updated from {before} to {after}. New MaxValue: {_slider.maxValue}");
    }

    // スライダーの値を更新
    private void UpdateSliderValues()
    {
        if (_currentAmountInfo == null) return;
        _slider.maxValue = (_maxAmountInfo != null) ? _maxAmountInfo.CurrentAmount : _currentAmountInfo.DefaultAmount;
        _slider.value = _currentAmountInfo.CurrentAmount;
    }

    /// <summary> イベントの購読を解除する </summary>
    private void UnsubscribeFromEvents()
    {
        if (_currentAmountInfo != null)
        {
            _currentAmountInfo.OnAmountChanged -= OnCurrentAmountChanged;
        }

        if (_maxAmountInfo != null)
        {
            _maxAmountInfo.OnAmountChanged -= OnMaxAmountChanged;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}

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
        // インスペクターでターゲットが入っている場合はここでセットアップできる
        if (_targetUnit != null || _currentAmountInfo == null)
        {
            Setup(_targetUnit);
        }
    }

    // 外部からのセットアップ用
    public void Setup(UnitBase target)
    {
        if (target == null)
        {
            Debug.LogError("StatusSlider: Target Unit is not assigned.");
            return;
        }

        // 既存の購読を削除
        UnsubscribeFromEvents();

        _targetUnit = target;

        // 現在値のStatusInfoを取得し、イベントを購読
        if (_targetUnit.statusManager.TryGetStatus(_status, out _currentAmountInfo))
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
            if (_targetUnit.statusManager.TryGetStatus(Status.MaxHP, out _maxAmountInfo))
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
        if (_currentAmountInfo != null)
        {
            _slider.value = _currentAmountInfo.CurrentAmount;
        }
        // Debug.Log($"StatusSlider: MaxHP updated from {before} to {after}. New MaxValue: {_slider.maxValue}");
    }

    // スライダーの値を更新
    private void UpdateSliderValues()
    {
        if (_currentAmountInfo == null) return;

        // 最大値の設定
        if (_status == Status.HP && _maxAmountInfo != null)
        {
            _slider.maxValue = _maxAmountInfo.CurrentAmount;
         }
        else
        {
            _slider.maxValue = _currentAmountInfo.CurrentAmount;
        }

        // 現在地の設定
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

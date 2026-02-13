using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステータスの値をテキストで表示
/// </summary>
public class StatusAmountView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private UnitBase _targetUnit;
    [SerializeField] private Status _viewStatus;
    [SerializeField] private string _prefix = "";

    private Status _status => _viewStatus;
    private StatusInfo _maxAmountInfo;
    private StatusInfo _currentAmountInfo;

    private void Start()
    {
        if (_targetUnit != null || _currentAmountInfo == null)
        {
            Setup(_targetUnit);
        }
    }

    public void Setup(UnitBase target)
    {
        if (target == null)
        {
            Debug.LogError("StatusAmountView: Target Unit is not assigned.");
            return;
        }

        // 既存の購読を削除
        UnsubscribeFromEvents();

        _targetUnit = target;

        // 現在値のStatusInfoを取得し、イベントを購読
        if (_targetUnit.statusManager.TryGetStatus(_status, out _currentAmountInfo))
        {
            _currentAmountInfo.OnAmountChanged += HandleStatusChanged;
        }
        else
        {
            Debug.LogError($"StatusAmountView: {_status} is not registered in StatusManager.");
            return;
        }

        // 最大値のStatusInfoを取得し、イベントを購読（HPの場合、MaxHPが必要）
        if (_status == Status.HP)
        {
            if (_targetUnit.statusManager.TryGetStatus(Status.MaxHP, out _maxAmountInfo))
            {
                _maxAmountInfo.OnAmountChanged += HandleStatusChanged;
            }
        }

        // 初期値の設定
        UpdateTextView();

        Debug.Log($"StatusAmountView: Initialized for {_status} Current {_currentAmountInfo.CurrentAmount}");
    }

    // ステータス変更イベント購読
    private void HandleStatusChanged(float before, float after)
    {
        UpdateTextView();
    }

    // テキストの値を更新
    private void UpdateTextView()
    {
        if (_currentAmountInfo == null) return;

        if (_viewStatus == Status.HP && _maxAmountInfo != null)
        {
            _text.text = $"{_prefix}{Mathf.CeilToInt(_currentAmountInfo.CurrentAmount).ToString("F0")} / {Mathf.CeilToInt(_maxAmountInfo.CurrentAmount).ToString("F0")}";
        }
        else
        {
            // 現在地の設定
            _text.text = $"{_prefix}{_currentAmountInfo.CurrentAmount:F1}";

        }
    }

    /// <summary> イベントの購読を解除する </summary>
    private void UnsubscribeFromEvents()
    {
        if (_currentAmountInfo != null)
        {
            _currentAmountInfo.OnAmountChanged -= HandleStatusChanged;
        }

        if (_maxAmountInfo != null)
        {
            _maxAmountInfo.OnAmountChanged -= HandleStatusChanged;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
}

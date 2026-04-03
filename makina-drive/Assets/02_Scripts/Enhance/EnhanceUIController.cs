using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LayoutDirection { Up, Down }
public enum LayoutAlignment { Top, Center, Bottom }
public class EnhanceUIController : MonoBehaviour
{
    [SerializeField] private Freya _player;
    [SerializeField] private GameObject _uiPanel; // 選択画面のパネル
    [SerializeField] private List<EnhanceData> _allEnhanceList; // 全強化データ
    [SerializeField] private Button _closeUIButton;
    [SerializeField] private VisualInfo _ApplySE; // 強化時のSE
    [SerializeField] private VisualInfo _CancelSE; // 強化を表示できない場合のSE

    [Header("Status Summary Settings")]
    [SerializeField] private StatusDisplayConfiguration _displayConfig;
    [SerializeField] private Transform _statusSummaryContainer; // UIを並べる親
    [SerializeField] private StatusSummarySlot _statusSlotPrefab; // 1行分のUIプレハブ
    [SerializeField] private LayoutDirection _direction = LayoutDirection.Down;
    [SerializeField] private LayoutAlignment _alignment = LayoutAlignment.Center;
    [SerializeField] private float _spacing = 50f;                // プレハブ同士の間隔

    // UI側の各スロット（3択分）
    [SerializeField] private List<EnhanceChoiceSlot> _slots;

    private EnhanceManager _manager;
    private EnhanceApplier _applier;
    private EnhanceViewModelFactory _factory;
    private StatusSummaryProvider _summaryProvider;
    private List<StatusSummarySlot> _activeStatusSlots = new();
    private bool _isStop;
    public event Action OnEnhanceApply;

    private void Awake()
    {
        _manager = new EnhanceManager(_allEnhanceList);
        _factory = new EnhanceViewModelFactory();
        _uiPanel.SetActive(false);
        _closeUIButton.onClick.AddListener(() => OnCloseBottonClicked());
    }

    private void Start()
    {
        // Startで購読することで、Freya側の初期化完了を待つ
        _player.OnEnhancementRequest += OpenUI;
        GameStateManager.OnStateChanged += HandleStateChanged;

        _summaryProvider = new StatusSummaryProvider(_player.statusManager, _player._inventory);
        ApplySettings();
    }

    // × OnEnableだと初期化を待てない
    // private void OnEnable()
    // {
    //     _player._level.OnLevelUp += HandleOpenLevelUpUI;
    //     GameStateManager.OnStateChanged += HandleStateChanged;
    // }

    // 購読解除
    private void OnDisable()
    {
        _player.OnEnhancementRequest -= OpenUI;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // UIを表示
    private void OpenUI(int currentPoints)
    {
        if (_uiPanel.activeSelf) return;

        // 選択肢取得
        var choices = _manager.GetRandomChoices(_player._inventory, 3);

        // 選択肢が一つもない場合（全強化のLvがMax）
        if (choices == null || choices.Count == 0)
        {
            Debug.Log("選択可能な強化項目がありません。");
            if (SoundManager.instance != null) SoundManager.instance.PlaySE(_CancelSE.SEName, _CancelSE.Volume);
            return;
        }

        GameStateManager.instance.ChangeState(GameState.EnhanceSelect);
        _uiPanel.SetActive(true);
        Time.timeScale = 0;
        _applier = new EnhanceApplier(_player.statusManager, _player._inventory, _player.recoveryStatus);

        RefreshUI();
        UpdateStatusSummary();
    }

    // 選択肢を生成して表示（更新）
    private void RefreshUI()
    {
        // 選択肢取得
        var choices = _manager.GetRandomChoices(_player._inventory, 3);

        // 選択肢が一つもない場合（全強化のLvがMax）
        if (choices == null || choices.Count == 0)
        {
            Debug.Log("選択可能な強化項目がありません。");
            CloseUI();
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < choices.Count)
            {
                var viewModel = _factory.Create(choices[i], _player._inventory);
                _slots[i].Setup(viewModel, choices[i], OnChoiceSelected);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                // 選択肢の数がslots以下の場合は下の要素は非表示
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    // ステータス表示の更新
    private void UpdateStatusSummary()
    {
        var summaries = _summaryProvider.GetSummary(new List<StatusDisplayConfiguration> { _displayConfig });

        foreach (var slot in _activeStatusSlots) slot.gameObject.SetActive(false);

        // スロットの生成、更新
        for (int i = 0; i < summaries.Count; i++)
        {
            if (i >= _activeStatusSlots.Count)
            {
                _activeStatusSlots.Add(Instantiate(_statusSlotPrefab, _statusSummaryContainer));
            }

            _activeStatusSlots[i].Setup(summaries[i]);
            _activeStatusSlots[i].gameObject.SetActive(true);
        }

        // レイアウト計算
        ApplyLayout(_activeStatusSlots);
    }

    // ステータス表示スロットのレイアウトの計算
    private void ApplyLayout(List<StatusSummarySlot> slots)
    {
        int Count = slots.Count;
        if (Count == 0) return;

        // 全体の高さ
        float totalHeight = (Count - 1) * _spacing;

        // 開始位置のオフセット計算
        float startOffset = _alignment switch
        {
            LayoutAlignment.Top => 0,
            LayoutAlignment.Center => totalHeight * 0.5f,
            LayoutAlignment.Bottom => totalHeight,
            _ => 0
        };

        // 方向
        float dirMultiplier = _direction == LayoutDirection.Down ? -1f : 1f;

        // スロットの位置をセット
        for (int i = 0; i < Count; i++)
        {
            float yPos = (startOffset - (i * _spacing)) * dirMultiplier;
            slots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos);
        }
    }


    // ボタン選択後の処理
    private void OnChoiceSelected(EnhanceData selectedData)
    {
        // 強化を適用
        _applier.Apply(selectedData);

        // ステータス表示更新
        UpdateStatusSummary();

        if (SoundManager.instance != null) SoundManager.instance.PlaySE(_ApplySE.SEName, _ApplySE.Volume);

        // 強化権を消費
        _player._level.ConsumeEnhancementPoint();

        // 選択肢をリセット
        _manager.ResetChoices();

        // 強化権があれば続けて表示
        if (_player._level.EnhancementPoints > 0)
        {
            RefreshUI();
        }
        else
        {
            // UIを閉じて再開
            CloseUI();
        }
    }

    // UIを閉じる
    private void CloseUI()
    {
        _uiPanel.SetActive(false);
        GameStateManager.instance.ChangeState(GameState.Play);
        Time.timeScale = 1;
        OnEnhanceApply?.Invoke();
    }
    // 閉じるボタンが押された時
    public void OnCloseBottonClicked()
    {
        CloseUI();
    }

    // クリア後は表示処理を停止させる
    private void HandleStateChanged(GameState gameState)
    {
        if (gameState == GameState.Clear)
        {
            _isStop = true;
        }
    }

    // 設定を反映させる
    private void ApplySettings()
    {
        if (VisualSettingsManager.instance != null)
        {
            // 即強化の設定がある場合は非表示
            if (VisualSettingsManager.instance.Settings.isImmediateEnhancement)
            {
                _closeUIButton.gameObject.SetActive(false);
            }
        }
    }
}

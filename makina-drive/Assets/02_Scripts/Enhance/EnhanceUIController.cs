using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUIController : MonoBehaviour
{
    [SerializeField] private Freya _player;
    [SerializeField] private GameObject _uiPanel; // 選択画面のパネル
    [SerializeField] private List<EnhanceData> _allEnhanceList; // 全強化データ
    [SerializeField] private Button _closeUIButton;

    // UI側の各スロット（3択分）
    [SerializeField] private List<EnhanceChoiceSlot> _slots;

    private EnhanceManager _manager;
    private EnhanceApplier _applier;
    private EnhanceViewModelFactory _factory;
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

        _uiPanel.SetActive(true);
        Time.timeScale = 0;
        _applier = new EnhanceApplier(_player.statusManager, _player._inventory);
        RefreshUI();
    }

    // 選択肢を生成して表示（更新）
    private void RefreshUI()
    {
        var choices = _manager.GetRandomChoices(_player._inventory, 3);

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
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    // ボタン選択後の処理
    private void OnChoiceSelected(EnhanceData selectedData)
    {
        // 強化を適用
        _applier.Apply(selectedData);

        // 強化権を消費
        _player._level.ConsumeEnhancementPoint();

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
        Time.timeScale = 1;
        OnEnhanceApply?.Invoke();
    }
    // 閉じるボタンが押された時
    public void OnCloseBottonClicked()
    {
        CloseUI();
    }

    // クリア後は表示を停止させる
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

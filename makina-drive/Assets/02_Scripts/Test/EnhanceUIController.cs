using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUIController : MonoBehaviour
{
    [SerializeField] private Freya _player;
    [SerializeField] private GameObject _uiPanel; // 選択画面のパネル
    [SerializeField] private List<EnhanceData> _allEnhanceList; // 全強化データ

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
    }

    private void Start()
    {
        // Startで購読することで、Freya側の初期化完了を待つ
        _player._level.OnLevelUp += HandleOpenLevelUpUI;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    // private void OnEnable()
    // {
    //     _player._level.OnLevelUp += HandleOpenLevelUpUI;
    //     GameStateManager.OnStateChanged += HandleStateChanged;
    // }

    // 購読解除
    private void OnDisable()
    {
        _player._level.OnLevelUp -= HandleOpenLevelUpUI;
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleOpenLevelUpUI(int currentLevel)
    {
        if (_isStop) return;
        // ゲームを一時停止
        Time.timeScale = 0;
        _uiPanel.SetActive(true);

        _applier = new EnhanceApplier(_player.statusManager, _player._inventory);

        var choicces = _manager.GetRandomChoices(_player._inventory, 3);

        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < choicces.Count)
            {
                var viewModel = _factory.Create(choicces[i], _player._inventory);
                _slots[i].Setup(viewModel, choicces[i], OnChoiceSelected);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void HandleStateChanged(GameState gameState)
    {
        if (gameState == GameState.Clear)
        {
            _isStop = true;
        }
    }

    private void OnChoiceSelected(EnhanceData selectedData)
    {
        // 強化を適用
        _applier.Apply(selectedData);

        // UIを閉じて再開
        _uiPanel.SetActive(false);
        Time.timeScale = 1;
        OnEnhanceApply?.Invoke();
    }
}

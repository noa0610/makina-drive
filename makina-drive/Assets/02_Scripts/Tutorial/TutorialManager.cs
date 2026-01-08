using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// チュートリアル管理用クラス
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialStepData> _steps;
    [SerializeField] private Freya _player;
    [SerializeField] private TutorialUI _ui;
    [SerializeField] private TutorialArrow _arrow;

    private IntReactiveProperty _currenActiontCount = new IntReactiveProperty(0);
    private int _currentStepIndex = 0;

    private CancellationTokenSource _cts;

    void Start()
    {
        _cts = new CancellationTokenSource();

        _currenActiontCount.Subscribe(count =>
        {
            if (_steps.Count > _currentStepIndex)
                _ui.UpdateCountText(count, _steps[_currentStepIndex].taskCount);
        }).AddTo(this);

        _player.stateMachine.OnStateChanged += HandlePlayerStateChanged;

        SetupStepAsync(0).Forget();
    }


    private async UniTask SetupStepAsync(int index)
    {
        var step = _steps[index];
        _currenActiontCount.Value = 0;

        // 無敵化設定
        _player.SetInvincible(step.isInvincible);

        // ウィンドウ表示
        if (step.showExplanationWindow && step.stopGameDuringWindow)
        {
            GameStateManager.instance.ChangeState(GameState.Pause);
            Time.timeScale = 0;
        }

        // UI表示
        await _ui.ShowStepVisualsAsync(step, _cts.Token);

        // ゲーム再開
        if (Time.timeScale == 0)
        {
            GameStateManager.instance.ChangeState(GameState.Play);
            Time.timeScale = 1;
        }

        // 矢印表示
        if (step.conditionType == TutorialConditionType.MoveToArea)
            _arrow.SetTarget(step.targetPoint);

        else
            _arrow.SetTarget(Vector3.zero);
    }



    // ステートのタグを識別してカウント
    private void HandlePlayerStateChanged(StateInfo state)
    {
        var currentStep = _steps[_currentStepIndex];
        if (currentStep.conditionType != TutorialConditionType.PerformAction) return;

        // ステートにStepData内のタグが含まれているかチェック
        if (state.HasTag(currentStep.targetStateTag))
        {
            AddCount();
        }
    }

    public void AddCount()
    {
        _currenActiontCount.Value++;

        if (_currenActiontCount.Value >= _steps[_currentStepIndex].taskCount)
        {
            NextStepAsync().Forget();
        }
    }

    private async UniTaskVoid NextStepAsync()
    {
        await _ui.HideTaskHUDAsync();

        _currentStepIndex++;
        if (_currentStepIndex < _steps.Count)
        {
            await SetupStepAsync(_currentStepIndex);
        }
        else
        {
            FinishTutorial();
        }
    }


    private void FinishTutorial()
    {
        Debug.Log("チュートリアル完了！");
        _player.SetInvincible(false);
    }


    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        // メモリリーク防止のため購読解除
        if (_player != null && _player.stateMachine != null)
            _player.stateMachine.OnStateChanged -= HandlePlayerStateChanged;
    }
}

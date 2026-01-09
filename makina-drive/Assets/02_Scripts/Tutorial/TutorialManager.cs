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
    [SerializeField] private TutorialTriggerArea _aria;

    [Header("SE")]
    [SerializeField] private VisualInfo _countUpSE;
    [SerializeField] private VisualInfo _stepClearSE;

    private IntReactiveProperty _currenActiontCount = new IntReactiveProperty(0);
    private int _currentStepIndex = 0;
    private List<TutorialTriggerArea> _spawnedTriggers = new List<TutorialTriggerArea>();

    private CancellationTokenSource _cts;

    void Start()
    {
        _cts = new CancellationTokenSource();

        // カウントが変わるたびに自動でUIテキストを更新予約
        _currenActiontCount.Subscribe(count =>
        {
            if (_steps.Count > _currentStepIndex)
                _ui.UpdateCountText(count, _steps[_currentStepIndex].taskCount);
        }).AddTo(this);

        _player.stateMachine.OnStateChanged += HandlePlayerStateChanged;

        // 最初のチュートリアルステップ表示
        SetupStepAsync(0).Forget();
    }

    // エリア侵入処理
    public void OnAreaReached(Vector3 position)
    {
        // 矢印のターゲットから除外
        _arrow.RemoveTarget(position);
        AddCount();
    }

    // チュートリアルステップセットアップ
    private async UniTask SetupStepAsync(int index)
    {
        var step = _steps[index];
        _currenActiontCount.Value = 0;

        ClearActiveTriggers();

        // 無敵化設定
        _player.SetInvincible(step.isInvincible);

        bool wasPaused = false;

        // ウィンドウ表示
        if (step.showExplanationWindow && step.stopGameDuringWindow)
        {
            GameStateManager.instance.ChangeState(GameState.Pause);
            // Time.timeScale = 0;
            wasPaused = true;
        }

        // UI表示
        await _ui.ShowStepVisualsAsync(step, _cts.Token);
        Debug.Log("WindowClose");

        // ゲーム再開
        if (wasPaused)
        {
            Debug.Log("TimeReStart");
            GameStateManager.instance.ChangeState(GameState.Play);
            Time.timeScale = 1;
        }


        // 矢印表示
        if (step.conditionType == TutorialConditionType.MoveToArea)
        {
            _arrow.gameObject.SetActive(true);
            foreach (var pos in step.targetPoint)
            {
                var trigger = Instantiate(_aria, pos, Quaternion.identity);
                trigger.SetManager(this);
                _spawnedTriggers.Add(trigger);
            }
            _arrow.SetTargets(step.targetPoint);
        }
        else
        {
            _arrow.gameObject.SetActive(false);
        }
    }

    private void ClearActiveTriggers()
    {
        foreach (var t in _spawnedTriggers)
        {
            if (t != null) Destroy(t.gameObject);
        }
        _spawnedTriggers.Clear();
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

    // カウントアップ
    public void AddCount()
    {
        _currenActiontCount.Value++;

        if (_countUpSE.SEName != null) SoundManager.instance.PlaySE(_countUpSE.SEName, _countUpSE.Volume);

        if (_currenActiontCount.Value >= _steps[_currentStepIndex].taskCount)
        {
            if (_stepClearSE.SEName != null) SoundManager.instance.PlaySE(_stepClearSE.SEName, _stepClearSE.Volume);
            CompleteStepAsync().Forget();
        }
    }

    private async UniTaskVoid CompleteStepAsync()
    {
        await _ui.ShowSuccessFeedbackAsync(_cts.Token);
        NextStepAsync().Forget();
    }

    // 次のステップへ
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
            // 最後のステップなら終了処理
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

using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Threading.Tasks;

/// <summary>
/// チュートリアル管理用クラス
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialStepData> _steps;
    [SerializeField] private Freya _player;
    [SerializeField] private EnhanceUIController _enhanceUI;
    [SerializeField] private TutorialUI _ui;
    [SerializeField] private TutorialArrow _arrow;
    [SerializeField] private TutorialTriggerArea _aria;
    [SerializeField] private TutorialEnemySpawner _enemySpawner;

    [Header("SE")]
    [SerializeField] private VisualInfo _countUpSE;
    [SerializeField] private VisualInfo _stepClearSE;

    private IntReactiveProperty _currenActiontCount = new IntReactiveProperty(0);
    private int _currentStepIndex = 0;
    private List<TutorialTriggerArea> _spawnedTriggers = new List<TutorialTriggerArea>();
    private bool _isSetUp = false;   // セットアップの重複防止用
    private bool _taskClear = false; // タスククリア判定の重複防止用

    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();

        _arrow.gameObject.SetActive(false);

        // カウントが変わるたびに自動でUIテキストを更新予約
        _currenActiontCount.Subscribe(count =>
        {
            if (_steps.Count > _currentStepIndex)
                _ui.UpdateCountText(count, _steps[_currentStepIndex].taskCount);
        }).AddTo(this);

        // プレイヤーのステート変化処理を登録
        _player.stateMachine.OnStateChanged += HandlePlayerStateChanged;

        // 敵被弾処理を登録
        UnitManager.OnUnitDamaged -= HandleUnitDamaged;
        UnitManager.OnUnitDamaged += HandleUnitDamaged;

        // 敵撃破処理を登録
        _enemySpawner.OnEnemyDefeated += HandleEnemyDefeated;

        // 敵全滅処理を登録
        _enemySpawner.OnAllEnemyDead += () =>
        {
            if (_steps.Count <= _currentStepIndex) return;

            var step = _steps[_currentStepIndex];

            // 現在のステップが敵生成を行う設定でない場合は全滅イベントを無視
            if (step.spawneEnemy == null || step.spawneEnemy.unitBase == null)
            {
                return;
            }

            if (!_taskClear)
            {
                RetryStepAsync().Forget();
            }
        };

        // レベルアップ強化適用処理を登録
        _enhanceUI.OnEnhanceApply += HandleEnhanceApply;

        // 最初のチュートリアルステップ表示
        SetupStepAsync(0).Forget();
    }

    // チュートリアルステップセットアップ
    private async UniTask SetupStepAsync(int index)
    {
        if (_isSetUp)
        {
            Debug.Log("セットアップ途中に他のセットアップの処理が行われました。");
            return;
        }
        _isSetUp = true;
        try
        {
            _taskClear = false;
            var step = _steps[index];
            _currenActiontCount.Value = 0;

            ClearActiveTriggers();

            // 敵生成初期化
            _enemySpawner.DestroyAllEnemy();
            _enemySpawner.StopSpawning();

            if (step.spawneEnemy != null && step.spawneEnemy.unitBase != null)
            {
                // 敵を生成
                _enemySpawner.StartSpawn(step.spawneEnemy, _player.gameObject);
            }

            // 無敵化設定
            _player.SetInvincible(step.isInvincible);

            bool wasPaused = false;

            // ウィンドウ表示
            if (step.showExplanationWindow && step.stopGameDuringWindow)
            {
                GameStateManager.instance.ChangeState(GameState.TutorialPause);
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
                GameStateManager.instance.ChangeState(GameState.TutorialPlay);
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
        finally
        {
            _isSetUp = false;
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
    
    // エリア侵入処理
    public void OnAreaReached(Vector3 position)
    {
        // 矢印のターゲットから除外
        _arrow.RemoveTarget(position);
        AddCount();
    }


    #region    ===== Event Handle =====

    // ステートのタグを識別してカウント
    private void HandlePlayerStateChanged(StateInfo state)
    {
        var step = _steps[_currentStepIndex];
        if (step.conditionType != TutorialConditionType.PerformAction) return;

        // ステートにStepData内のタグが含まれているかチェック
        if (state.HasTag(step.targetStateTag))
        {
            AddCount();
        }
    }

    // ダメージイベントを受け取りカウント
    private void HandleUnitDamaged(UnitBase target, UnitBase attacker, BulletStatus? status)
    {
        if (_steps.Count <= _currentStepIndex || _taskClear) return;
        var step = _steps[_currentStepIndex];

        // 攻撃側はプレイヤーのみ
        if (attacker != _player) return;

        bool isMatched = false;

        switch (step.conditionType)
        {
            // 攻撃タグが一致するか
            case TutorialConditionType.HitAttackTag:
                if (status.HasValue)
                    foreach (string tag in status.Value.attackTag)
                    {
                        // 一致するものが一つでもあるか
                        if (tag == step.targetAttackTag)
                        {
                            isMatched = true;
                            break;
                        }
                    }
                break;
            // ユニット名が一致するか
            case TutorialConditionType.DamageSpecificUnit:
                if (target.UnitStatusData.unitName == step.targetUnitName)
                    isMatched = true;
                break;
            // ユニットタグが含まれるか
            case TutorialConditionType.DamageUnitWithTag:
                if (target.UnitStatusData.tags == step.targetUnitTag)
                    isMatched = true;
                break;
        }

        if (isMatched)
        {
            AddCount();
        }
    }

    // 敵撃破イベントを受け取りカウント
    private void HandleEnemyDefeated(UnitBase unit)
    {
        if (_steps.Count <= _currentStepIndex || _taskClear) return;
        var step = _steps[_currentStepIndex];

        if (step.conditionType != TutorialConditionType.DefeatEnemy) return;

        bool isMatched = true;

        // // TODO 名前指定がある場合
        // if (!string.IsNullOrEmpty(step.targetUnitName) && unit.UnitStatusData.unitName != step.targetUnitName)
        // {
        //     // 一致しない
        //     isMatched = false;
        // }

        // // TODO タグ指定がある場合
        // if (step.targetUnitTag != 0 && unit.UnitStatusData.tags != step.targetUnitTag)
        // {
        //     // 一致しない
        //     isMatched = false;
        // }

        if (isMatched)
        {
            AddCount();
        }
    }

    // 強化イベントを受け取りカウント
    private void HandleEnhanceApply()
    {
        if (_steps.Count <= _currentStepIndex || _taskClear) return;
        var step = _steps[_currentStepIndex];

        if (step.conditionType != TutorialConditionType.EnhanceApply) return;

        AddCount();
    }

    #endregion


    #region    ===== Tutorial Step Up =====
    // カウントアップ
    public void AddCount()
    {
        if (_taskClear) return;

        _currenActiontCount.Value++;

        if (_countUpSE.SEName != null) SoundManager.instance.PlaySE(_countUpSE.SEName, _countUpSE.Volume);

        if (_currenActiontCount.Value >= _steps[_currentStepIndex].taskCount && _taskClear == false)
        {
            if (_stepClearSE.SEName != null) SoundManager.instance.PlaySE(_stepClearSE.SEName, _stepClearSE.Volume);
            CompleteStepAsync().Forget();
            _taskClear = true;
        }
    }

    // クリア処理
    private async UniTaskVoid CompleteStepAsync()
    {
        _arrow.gameObject.SetActive(false);
        // クリアUIを表示
        await _ui.ShowSuccessFeedbackAsync(_cts.Token);
        NextStepAsync().Forget();
    }

    // リトライ処理
    private async UniTaskVoid RetryStepAsync()
    {
        var step = _steps[_currentStepIndex];

        if (step.showFailureWindow)
        {
            // 失敗UIを表示
            await _ui.ShowFailureFeedbackAsync(_cts.Token);
        }

        // 現在のステップを最初からやり直す
        await SetupStepAsync(_currentStepIndex);
    }

    // 次のステップへ
    private async UniTaskVoid NextStepAsync()
    {
        await _ui.HideTaskHUDAsync();

        _enemySpawner.DestroyAllEnemy();

        _currentStepIndex++;
        if (_currentStepIndex < _steps.Count)
        {
            await SetupStepAsync(_currentStepIndex);
        }
        else
        {
            // 最後のステップなら終了処理
            await FinishTutorial();
        }
    }

    // チュートリアル終了
    private async Task FinishTutorial()
    {
        Debug.Log("チュートリアル完了！");
        if (_player != null)
        {
            _player.stateMachine.OnStateChanged -= HandlePlayerStateChanged;
            _player.SetInvincible(false);
        }
        _enemySpawner.DestroyAllEnemy();
        _enemySpawner.StopSpawning();

        await _ui.ShowStepAllClearVisualsAsync();
        GameStateManager.instance.ChangeState(GameState.Clear);
    }
    #endregion


    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        // メモリリーク防止のため購読解除
        if (_player != null && _player.stateMachine != null)
            _player.stateMachine.OnStateChanged -= HandlePlayerStateChanged;
        UnitManager.OnUnitDamaged -= HandleUnitDamaged;
        _enhanceUI.OnEnhanceApply -= HandleEnhanceApply;
    }
}

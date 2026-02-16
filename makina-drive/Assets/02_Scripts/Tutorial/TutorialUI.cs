using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using NUnit.Framework;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Groups")]
    [SerializeField] private CanvasGroup _windowGroup;  // 説明ウィンドウ用
    [SerializeField] private CanvasGroup _taskGroup;    // タスク進行テキスト用
    [SerializeField] private CanvasGroup _successGroup; // タスククリアテキスト用
    [SerializeField] private CanvasGroup _failureGroup; // タスク失敗テキスト用
    [SerializeField] private CanvasGroup _taskAllClearGroup; // 全タスク達成テキスト用

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _windowText;
    [SerializeField] private TextMeshProUGUI _centerText;
    [SerializeField] private TextMeshProUGUI _subText;
    [SerializeField] private TextMeshProUGUI _countText;  // 「残り○回」の表示用
    [SerializeField] private TextMeshProUGUI _successText;
    [SerializeField] private TextMeshProUGUI _failureText;


    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private float _displayDelay = 0.2f;
    [SerializeField] private bool _activeChange = false;

    private void Awake()
    {
        // UIを透明化
        SetGroupAlpha(_windowGroup, 0);
        SetGroupAlpha(_taskGroup, 0);
        SetGroupAlpha(_successGroup, 0);
        SetGroupAlpha(_failureGroup, 0);
        SetGroupAlpha(_taskAllClearGroup, 0);
    }

    private void SetGroupAlpha(CanvasGroup group, float alpha)
    {
        group.alpha = alpha;
        if (_activeChange)
        {
            group.gameObject.SetActive(alpha > 0);
        }
    }

    private async UniTask FadeAsync(CanvasGroup group, float targetAlpha, float duration)
    {
        if (_activeChange && targetAlpha > 0)
        {
            group.gameObject.SetActive(true);
        }

        float startAlpha = group.alpha;
        float time = 0;
        if (duration <= 0)
        {
            group.alpha = targetAlpha;
        }
        else
        {
            while (time < duration)
            {
                // ポーズ中でも動くunscaledDeltaTime
                time += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                await UniTask.Yield();
            }
            group.alpha = targetAlpha;
        }
        group.interactable = targetAlpha > 0;
        group.blocksRaycasts = targetAlpha > 0;

        if(_activeChange && targetAlpha <= 0)
        {
            group.gameObject.SetActive(false);
        }
    }

    // 説明ウィンドウ表示
    public async UniTask ShowStepVisualsAsync(TutorialStepData step, CancellationToken ct)
    {
        _windowText.text = step.windowText;
        _centerText.text = step.centerText;
        _subText.text = step.subText;

        if (step.showExplanationWindow)
        {
            // ウィンドウ表示
            await FadeAsync(_windowGroup, 1, _fadeDuration);
            
            // 何らかのキー入力
            await UniTask.WaitUntil(() => Input.anyKeyDown, cancellationToken: ct);
            Debug.Log("windowEnd");

            // ウィンドウを閉じる
            await FadeAsync(_windowGroup, 0, _fadeDuration);
            await UniTask.Delay(TimeSpan.FromSeconds(_displayDelay), ignoreTimeScale: true, cancellationToken: ct);
        }

        UpdateCountText(0, step.taskCount);
        await FadeAsync(_taskGroup, 1, _fadeDuration);
    }

    // タスク内容表示
    public async UniTask HideTaskHUDAsync()
    {
        await FadeAsync(_taskGroup, 0, _fadeDuration);
    }

    // 残りタスク数表示
    public void UpdateCountText(int current, int total)
    {
        int remaining = total - current;
        _countText.text = $"(あと {Mathf.Max(0, remaining)} 回)";
    }

    // タスク完了表示
    public async UniTask ShowSuccessFeedbackAsync(CancellationToken ct)
    {
        _successText.text = "OK!";
        await FadeAsync(_successGroup, 1, 0.1f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
        await FadeAsync(_successGroup, 0, 0.2f);
    }

    // タスク失敗表示
    public async UniTask ShowFailureFeedbackAsync(CancellationToken ct)
    {
        _failureText.text = "Miss…";
        await FadeAsync(_failureGroup, 1, 0.1f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
        await FadeAsync(_failureGroup, 0, 0.2f);
    }

    // 全タスク完了表示
    public async UniTask ShowStepAllClearVisualsAsync()
    {
        await FadeAsync(_taskAllClearGroup, 1, _fadeDuration);
    }
}

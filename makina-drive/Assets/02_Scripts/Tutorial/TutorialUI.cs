using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class TutorialUI : MonoBehaviour
{
    [Header("UI Groups")]
    [SerializeField] private CanvasGroup _windowGroup;  // 説明ウィンドウ用
    [SerializeField] private CanvasGroup _taskHUDGroup; // 進行テキスト用
    [SerializeField] private CanvasGroup _successGroup; // タスククリアテキスト用

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI _windowText;
    [SerializeField] private TextMeshProUGUI _centerText;
    [SerializeField] private TextMeshProUGUI _subText;
    [SerializeField] private TextMeshProUGUI _countText;  // 「残り○回」の表示用
    [SerializeField] private TextMeshProUGUI _successText;

    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private float _displayDelay = 0.2f;

    private void Awake()
    {
        // UIを透明化
        _windowGroup.alpha = 0;
        _taskHUDGroup.alpha = 0;
        _successGroup.alpha = 0;
    }

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
            await UniTask.Delay(TimeSpan.FromSeconds(_displayDelay), cancellationToken: ct);
        }

        UpdateCountText(0, step.taskCount);
        await FadeAsync(_taskHUDGroup, 1, _fadeDuration);
    }

    public async UniTask HideTaskHUDAsync()
    {
        await FadeAsync(_taskHUDGroup, 0, _fadeDuration);
    }

    public void UpdateCountText(int current, int total)
    {
        int remaining = total - current;
        _countText.text = $"(あと {remaining} 回)";
    }

    private async UniTask FadeAsync(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float time = 0;
        while (time < duration)
        {
            // ポーズ中でも動くunscaledDeltaTime
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            await UniTask.Yield();
        }
        group.alpha = targetAlpha;
        group.interactable = targetAlpha > 0;
        group.blocksRaycasts = targetAlpha > 0;
    }

    public async UniTask ShowSuccessFeedbackAsync(CancellationToken ct)
    {
        _successText.text = "OK!";
        await FadeAsync(_successGroup, 1, 0.1f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
        await FadeAsync(_successGroup, 0, 0.2f);
    }
}

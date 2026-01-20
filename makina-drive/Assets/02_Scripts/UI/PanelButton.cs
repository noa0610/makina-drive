using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class PanelButton : MonoBehaviour
{
    [SerializeField] private CanvasGroup _panelGroup;
    [SerializeField] private Button _inButton;
    [SerializeField] private Button _outButton;
    [SerializeField] private VisualInfo _SelectSE;

    
    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 0.3f;
    [SerializeField] private float _displayDelay = 0.2f;
    [SerializeField] private bool _activeChange = false;

    private void Start()
    {
        if(_inButton != null)
        {
            _inButton.onClick.AddListener(()=> FadeAsync(_panelGroup, 1, _fadeDuration).Forget());
        }

        if(_outButton != null)
        {
            _outButton.onClick.AddListener(()=> FadeAsync(_panelGroup, 0, _fadeDuration).Forget());
        }
        SetGroupAlpha(_panelGroup, 0);
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
        if (_SelectSE.SEName != null)
        {
            SoundManager.instance.PlaySE(_SelectSE.SEName, _SelectSE.Volume);
        }

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
}

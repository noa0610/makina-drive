using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System;

/// <summary>
/// GameStateの変化に合わせて指定されたUI（Graphic）要素の透明度を変化させるテスト用クラス
/// </summary>
public class TestGameStateViewUI : MonoBehaviour
{
    private enum FadeType
    {
        In,
        Out,
        FadeIn,
        FadeOut
    }
    [SerializeField] private FadeType _fadeType = FadeType.In;
    [SerializeField] private GameState _reactGameState;
    [SerializeField] private Graphic[] _fadeUI;
    [SerializeField] private float _Delay;
    [SerializeField] private float _fedeEffectTime;

    [SerializeField] private bool _activeChange;   // アクティブ状態も切り替えるか
    [SerializeField] private bool _changeOnlyOnce = false; // 表示切り替えは一度限り

    private int _changeCount = 0;


    // 状態変更イベントを購読
    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    // 購読解除
    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        if (_activeChange)
        {
            if (_fadeType == FadeType.In || _fadeType == FadeType.FadeIn)
            {
                SetGraphicsActive(_fadeUI, false);
            }
            else
            {
                SetGraphicsActive(_fadeUI, true);
            }
        }
    }

    // イベント変更時に処理を行うハンドル
    private void HandleStateChanged(GameState newState)
    {
        if (newState == _reactGameState)
        {
            if (_changeOnlyOnce && _changeCount > 0) return;

            ChangeGraphics();
            _changeCount++;
        }
    }

    // Graphic要素を表示
    private async void ChangeGraphics()
    {
        if (_Delay >= 0) await UniTask.Delay(TimeSpan.FromSeconds(_Delay));

        if (_activeChange)
        {
            if (_fadeType == FadeType.In || _fadeType == FadeType.FadeIn)
            {
                SetGraphicsActive(_fadeUI, true);
            }

        }

        if (_fadeType == FadeType.In) FadeInGraphics(_fadeUI, 0);
        else if (_fadeType == FadeType.Out) FadeOutGraphics(_fadeUI, 0);
        else if (_fadeType == FadeType.FadeIn) FadeInGraphics(_fadeUI, _fedeEffectTime);
        else if (_fadeType == FadeType.FadeOut) FadeOutGraphics(_fadeUI, _fedeEffectTime);

        if (_activeChange)
        {
            if (_fadeType == FadeType.Out || _fadeType == FadeType.FadeOut)
            {
                SetGraphicsActive(_fadeUI, true);
            }
        }
    }

    // フェードイン
    private void FadeInGraphics(Graphic[] graphics, float fadeTime)
    {
        foreach (var graphic in graphics)
        {
            if (graphic != null)
            {
                StartCoroutine(UIFade_Transparent.FadeIn(graphic, fadeTime));
            }
        }
    }

    // フェードアウト
    private void FadeOutGraphics(Graphic[] graphics, float fadeTime)
    {
        foreach (var graphic in graphics)
        {
            if (graphic != null)
            {
                StartCoroutine(UIFade_Transparent.FadeOut(graphic, fadeTime));
            }
        }
    }

    // アクティブ状態切り替え
    private void SetGraphicsActive(Graphic[] graphics, bool isActive)
    {
        foreach (var graphic in graphics)
        {
            if (graphic != null)
            {
                graphic.gameObject.SetActive(isActive);
            }
        }
    }
}

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeButton : MonoBehaviour
{
    [SerializeField] private Button _transitionButton;
    [SerializeField] private string _targetSceneName = "";
    [SerializeField] private float _delaySeconds = 1.0f;
    [SerializeField] private VisualInfo _SelectSE;
    [SerializeField] private VisualInfo _notSelectSE;
    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();

        if (_transitionButton != null)
        {
            _transitionButton.onClick.AddListener(() => OnButtonClickAsync().Forget());
        }
    }

    public async UniTaskVoid OnButtonClickAsync()
    {
        if (string.IsNullOrEmpty(_targetSceneName))
        {
            if (_notSelectSE.SEName != null)
            {
                SoundManager.instance.PlaySE(_notSelectSE.SEName, _notSelectSE.Volume);
            }
            return;
        }


        if (_transitionButton != null)
        {
            _transitionButton.interactable = false;
        }

        Debug.Log("ボタンがクリックされました。シーン遷移まで遅延します。");

        if (_SelectSE.SEName != null)
        {
            SoundManager.instance.PlaySE(_SelectSE.SEName, _SelectSE.Volume);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(_delaySeconds));

        SceneManager.LoadScene(_targetSceneName);
    }
}

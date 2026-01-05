using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;

public class UI_Slide : MonoBehaviour
{
    [SerializeField]
    private Button _StartButton;
    [SerializeField]
    private Vector3 _Start_Pos;
    [SerializeField]
    private Vector3 _End_Pos;
    [SerializeField]
    private GameObject _Target;
    [SerializeField]
    private float _Time;
    [SerializeField]
    private Ease _ease = Ease.Unset;

    public event Action OnSlideComplete;

    void Start()
    {
        _StartButton.onClick.AddListener(OnStartButton);
        _Target.transform.localPosition = _Start_Pos;
    }

    public void OnStartButton()
    {
        _Target.transform.DOLocalMove(_End_Pos, _Time).SetEase(_ease).OnComplete(() =>
        {
            OnSlideComplete?.Invoke();
        });
        
        
    }
}

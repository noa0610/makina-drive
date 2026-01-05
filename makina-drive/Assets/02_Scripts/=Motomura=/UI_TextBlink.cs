using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UI_TextBlink : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _Target;
    [SerializeField]
    private float _Time;

    void Start()
    {
        _Target.DOFade(1, 0);
        _Target.DOFade(0.3f, _Time).SetLoops(-1,LoopType.Yoyo);
    }
}

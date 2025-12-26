using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LevelUpWindowUI : MonoBehaviour
{
    [SerializeField]
    private GameObject _levelUpWindow;
    [SerializeField]
    private List<Button> _ListButton;
    [SerializeField]
    private float _displaySpeed;
    [SerializeField]
    private float _hiddenSpeed;

    private bool Completeclose = true;
    
    void Start()
    {
        _levelUpWindow.transform.localPosition = new Vector3(0, 0, 0);
        _levelUpWindow.transform.localScale = Vector3.zero;
        
        foreach (var button in _ListButton)
        {
            button.onClick.AddListener(CloseLevelUpWindow);
        }
    }

    void CloseLevelUpWindow()
    {
        if (Completeclose)
        {         
            Completeclose = false;
            _levelUpWindow.transform.DOScale(Vector3.zero, _hiddenSpeed).SetEase(Ease.InBack).OnComplete(() => Completeclose = true);

        }
    }
    public void OpenLevelUpWindow()
    {
        _levelUpWindow.transform.DOScale(Vector3.one, _displaySpeed).SetEase(Ease.OutBack);
    }
}
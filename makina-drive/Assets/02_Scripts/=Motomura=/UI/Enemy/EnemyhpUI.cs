using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyhpUI : MonoBehaviour 
{
    [SerializeField]
    private GameObject _Enemy;
    private StatusManager _statusManager;
    [SerializeField]
    private Image _hpBar;
    private StatusInfo hpInfo;
    private StatusInfo MaxHPInfo;

    void Start()
    {
        _statusManager = _Enemy.GetComponent<UnitBase>().statusManager;
        hpInfo = _statusManager.GetStatus(Status.HP);
        MaxHPInfo = _statusManager.GetStatus(Status.MaxHP);


    }

void Update()
{

    switch (_Enemy.gameObject.transform.localScale.x)//Enemyの向きに合わせてHPバーの向きを変える
    {
        case -1:
            transform.localScale = new Vector3(-1, 1, 1);
            break;
        case 1:
            transform.localScale = new Vector3(1, 1, 1);
            break;
    }

    // HPが変化した時に自動で実行される処理を登録する   
    _hpBar.fillAmount = (float)hpInfo.CurrentAmount / (float)MaxHPInfo.CurrentAmount;
}
}

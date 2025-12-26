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
    var unit = _Enemy.GetComponent<UnitBase>();
    _statusManager = unit.statusManager;
    hpInfo = _statusManager.GetStatus(Status.HP);
    MaxHPInfo = _statusManager.GetStatus(Status.MaxHP);

    // HPが変わったときだけ UI を更新するように予約する
    hpInfo.OnAmountChanged += (before, after) => UpdateVisual();
    
    UpdateVisual(); // 初回表示
}

void Update()
{
    if (_Enemy == null) return;

    float side = Mathf.Sign(_Enemy.transform.localScale.x);
    transform.localScale = new Vector3(side, 1, 1);
}

private void UpdateVisual()
{
    if (MaxHPInfo.CurrentAmount > 0)
    {
        _hpBar.fillAmount = hpInfo.CurrentAmount / MaxHPInfo.CurrentAmount;
    }
}
}

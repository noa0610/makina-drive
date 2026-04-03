using UnityEngine;

// 経験値アイテム
public class ExpItem : DropItem
{
    [SerializeField] private float _expAmount;
    [SerializeField] private UnitBase _debugTarget;
    public void SetExp(float amount) => _expAmount = amount;

    private void Start()
    {
        if (_debugTarget != null)
        {
            Setup(_debugTarget);
        }
    }

    protected override void OnCollect(UnitBase target)
    {
        target.GainExp(_expAmount);
    }
}

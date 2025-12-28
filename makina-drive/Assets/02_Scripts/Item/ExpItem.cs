using UnityEngine;

// 経験値アイテム
public class ExpItem : DropItem
{
    private float _expAmount;
    public void SetExp(float amount) => _expAmount = amount;

    protected override void OnCollect(UnitBase target)
    {
        target.GainExp(_expAmount);
    }
}

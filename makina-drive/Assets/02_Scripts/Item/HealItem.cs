using UnityEngine;

// 回復アイテム
public class HealItem : DropItem
{
    private enum HealType
    {
        Fixed,
        Multiplier
    }
    [SerializeField] private HealType _type;
    [SerializeField] private float _healAmount;
    [SerializeField] private float _healRate;
    public void SetHealAmount(float amount) => _healAmount = amount;
    public void SetHealRate(float rate) => _healRate = rate;

    protected override void OnCollect(UnitBase target)
    {
        switch(_type)
        {
            case HealType.Fixed:
                target.statusManager.TakeHeal(_healAmount);
                break;
            case HealType.Multiplier:
                var maxHp = target.statusManager.ReadValue(Status.MaxHP); 
                target.statusManager.TakeHeal(maxHp * _healRate);
                break;
        }
        
    }
}

using UnityEngine;
using System.Collections.Generic;

public class UnitManager : SingletonBehavior<UnitManager>
{
    [Header("Debug")]
    [SerializeField] private bool _damegeLog;

    private List<UnitBase> _unitList = new List<UnitBase>();
    public void AddUnit(UnitBase unit)
    {
        _unitList.Add(unit);
    }
    public void RemoveUnit(UnitBase unit)
    {
        _unitList.Remove(unit);
    }
    public void Clear()
    {
        _unitList.Clear();
    }

    public List<UnitBase> GetUnitList()
    {
        return _unitList;
    }

    /// <summary>
    /// ダメージを与える
    /// </summary>
    /// <param name="target">ダメージを受ける側</param>
    /// <param name="from">ダメージを与える側</param>
    /// <param name="damage"></param>
    public void AddDamage(UnitBase target, UnitBase from, float damage,  Vector2 pushdir, float knockbackForce = 0)
    {
        float finalDamage = FinalDamageCalculation(damage, 
                                                   from.statusManager.ReadValue(Status.ATK), 
                                                   target.statusManager.ReadValue(Status.DEF), 
                                                   target.statusManager.ReadValue(Status.DamageRatio));
        if(_damegeLog) 
        {
            Debug.Log($"{target.name} : Take Damage {finalDamage}.  HP: {target.statusManager.ReadValue(Status.HP) - damage} /{target.statusManager.ReadValue(Status.MaxHP)}");
        }
        target.TakeDamage(from, finalDamage, pushdir, knockbackForce);
    }

    public void AddDamage(UnitBase target, UnitBase from, float damage)
    {
        float finalDamage = FinalDamageCalculation(damage, 
                                                   from.statusManager.ReadValue(Status.ATK), 
                                                   target.statusManager.ReadValue(Status.DEF), 
                                                   target.statusManager.ReadValue(Status.DamageRatio));
        if(_damegeLog) 
        {
            Debug.Log($"{target.name} : Take Damage {finalDamage}.  HP: {target.statusManager.ReadValue(Status.HP) - damage} /{target.statusManager.ReadValue(Status.MaxHP)}");
        }
        target.TakeDamage(from, finalDamage, Vector2.zero, 0);
    }

    // ダメージ計算式
    public float FinalDamageCalculation(float damage, float atkRate, float def, float damageRate)
    {
        float rn = Random.Range(0.9f, 1.1f); // 乱数
        float finalDamage = (damage * atkRate - def) * damageRate; // (弾ダメージ × 攻撃倍率 - 防御力) * 被ダメージ倍率
        return finalDamage * rn;
    }

    public void Pause(bool pause, bool isTimeStop = true)
    {
        UnitBase._isPlaying = !pause;
        if (isTimeStop)
            Time.timeScale = pause ? 0f : 1f;
        foreach (var unit in _unitList)
        {
            if (unit is IPausable pausable)
            {
                if (pause)
                    pausable.Pause();
                else
                    pausable.Play();
            }
        }
    }
}
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
    public void AddDamage(UnitBase target, IUnit from, float damage)
    {
        if(_damegeLog) 
        {
            Debug.Log($"{target.name} : Take Damage {damage}.  HP: {target.statusManager.ReadValue(Status.HP) - damage} /{target.statusManager.ReadValue(Status.MaxHP)}");
        }
        target.TakeDamage(from, damage);
    }

    /// <summary>
    /// 攻撃を受けた方向をセットする
    /// </summary>
    /// <param name="target">攻撃した側</param>
    /// <param name="from">攻撃された側</param>
    /// <param name="hitPoint"></param>
    public void AddAttackDirection(UnitBase target, UnitBase from, Vector2 hitPoint)
    {
        target.SetAttackerDirection(from, hitPoint);
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
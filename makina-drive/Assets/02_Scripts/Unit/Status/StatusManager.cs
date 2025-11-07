using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class StatusManager
{
    private Dictionary<Status, float> _statusAmount = new();
    private StatusManager Initialize(UnitStatusData data)
    {
        return this;
    }
    // private StateInfo AddStatus(Status status, float amount, bool isDynamic = true)
    // {
    //     _statusAmount[status, amount];
    //     return 
    // }

    private void TakeDamage(float damage)
    {
        
    }
}

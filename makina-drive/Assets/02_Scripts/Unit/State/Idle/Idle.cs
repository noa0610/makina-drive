using UnityEngine;

/// <summary>
/// 待機ステート
/// </summary>
public class Idle : StateComp
{
    public override void Stay(UnitBase parent, float deltaTime)
    {
        // Debug.Log($"deltaTime : {deltaTime}");
    }
}

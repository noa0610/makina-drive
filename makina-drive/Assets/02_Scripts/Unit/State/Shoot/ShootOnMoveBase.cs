using UnityEngine;

/// <summary>
/// 移動が可能な射撃ステート基底クラス
/// </summary>
public class ShootOnMoveBase : ShootStateBase
{
    public Rigidbody2D rigidbody2D { get; protected set; }
    protected Bullet instantiatedBullet;

    // === Constractor ===
    public ShootOnMoveBase(BulletData data, LayerMask targetLayer)
    {
        _data = data;
        _targetLayer = targetLayer; // レイヤーをセット
    }
    public ShootOnMoveBase() { }

    // === Public ===
    public void SetRB2(Rigidbody2D rb)
    {
        rigidbody2D = rb;
    }

    public override void Enter(IState preview, UnitBase parent)
    {
        base.Enter(preview, parent);
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
    }

    public override void Exit(IState nextState, UnitBase parent)
    {
        base.Exit(nextState, parent);
    }

    protected virtual Vector2 GetDirection(UnitBase unit) => unit.MoveDirection.normalized;
}

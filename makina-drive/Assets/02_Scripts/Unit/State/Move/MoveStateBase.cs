using UnityEngine;

public class MoveStateBase : StateComp, IRigidbodyUser
{
    public Rigidbody2D rigidbody2D { get; protected set; }

    public void SetRB2(Rigidbody2D rb)
    {
        rigidbody2D = rb;
    }

    protected virtual Vector2 GetDirection(UnitBase unit) => unit.Direction.normalized;
}

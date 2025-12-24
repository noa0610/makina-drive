using UnityEngine;

public class Idle_MoveStop : Idle
{
    public Rigidbody2D rigidbody2D { get; protected set; }

    public void SetRB2(Rigidbody2D rb)
    {
        rigidbody2D = rb;
    }

    public Idle_MoveStop(Rigidbody2D rb)
    {
        rigidbody2D = rb;
    }
    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        rigidbody2D.linearVelocity = Vector2.zero;
    }
}

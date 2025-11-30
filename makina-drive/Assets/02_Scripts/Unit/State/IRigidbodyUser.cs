using UnityEngine;

public interface IRigidbodyUser
{
    Rigidbody2D rigidbody2D { get; }

    void SetRB2(Rigidbody2D rb);
}
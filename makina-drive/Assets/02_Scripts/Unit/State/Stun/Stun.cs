using UnityEngine;
using System;

public class Stun : Idle_LazyChange
{
    [SerializeField] private float _knockbackForce = 5f;

    public Rigidbody2D _rigidbody2D;

    private Vector2 _knockbackDirection;

    public Stun(Rigidbody2D rigidbody2D, string lazyChange, float lazyChangeTime, bool isBlock = false) 
        : base (lazyChange, lazyChangeTime, isBlock = false)
    {
        _rigidbody2D = rigidbody2D;
        _lazyChange = lazyChange;
        _time = lazyChangeTime;
        _isBlock = isBlock;
    }

    public Stun(string lazyChange, float lazyChangeTime, bool isBlock = false) 
        : base (lazyChange, lazyChangeTime, isBlock = false)
    {
        _lazyChange = lazyChange;
        _time = lazyChangeTime;
        _isBlock = isBlock;
    }

    public override void Enter(IState previousIState, UnitBase parent)
    {
        base.Enter(previousIState, parent);
        if (_rigidbody2D != null)
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.AddForce(_knockbackDirection * _knockbackForce, ForceMode2D.Impulse);
        }
    }

    public void SetKnockbackDirection(Vector2 direction)
    {
        _knockbackDirection = direction.normalized;
    }

    public void SetKnockbackForce(float force)
    {
        _knockbackForce = force;
    }

    public void SetRB2(Rigidbody2D rigidbody2D)
    {
        _rigidbody2D = rigidbody2D;
    }
}

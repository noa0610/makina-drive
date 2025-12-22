using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Blowback : Idle_LazyChange
{
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private float _maxDistance;
    [SerializeField] private float _minAttackSpeed = 2.0f; // 攻撃が有効な最小速度
    
    private Vector2 _startPos;
    private Rigidbody2D _rb;

    public Blowback(Rigidbody2D rigidbody2D, float power, Vector2 direction, string lazyChange, float lazyChangeTime, bool isBlock = false) 
        : base (lazyChange, lazyChangeTime, isBlock = false)
    {
        _velocity = direction * power;
        _rb = rigidbody2D;
        _lazyChange = lazyChange;
        _time = lazyChangeTime;
        _isBlock = isBlock;
    }

    public void SetMaxDistance(float maxDistance)
    {
        _maxDistance = maxDistance;
    }

    public void SetVelocity(float power, Vector2 direction)
    {
        _velocity = direction * power;
    }

    public void SetRB2(Rigidbody2D rigidbody2D)
    {
        _rb = rigidbody2D;
    }

    public override void Enter(IState previousIState, UnitBase parent)
    {
        _startPos = parent.transform.position;

        _rb.linearVelocity = _velocity;
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, deltaTime * 5f);

        // 速度がほぼ0なら遷移
        if(_rb.linearVelocity.magnitude < 0.1f)
        {
            parent.stateMachine.LazyChange(_lazyChange);
        }

        // 移動距離が最大をこえたら遷移
        if(Vector2.Distance(_startPos, parent.transform.position) >= _maxDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            parent.stateMachine.LazyChange(_lazyChange);
        }

        if(_rb.linearVelocity.magnitude > _minAttackSpeed)
        {
            ChackCollisionWithOthers(parent);
        }
    }

    public override void Exit(IState nextState, UnitBase parent)
    {
        _rb.linearVelocity = Vector2.zero;
    }

    private void ChackCollisionWithOthers(UnitBase parent)
    {
        // 自分の周囲の敵を検知
        Collider2D[] hits = Physics2D.OverlapCircleAll(parent.transform.position, 0.5f, parent.AttackLayer);

        foreach (var hit in hits)
        {
            if (hit.gameObject == parent.gameObject) continue;

            if (hit.TryGetComponent<UnitBase>(out var target))
            {
                // 自分の攻撃力で相手にダメージを与える
                float damage = parent.UnitStatusData.atk;
                UnitManager.instance.AddDamage(target, parent, damage);

                // TODO ヒットした相手へのノックバック
            }
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Blowback : Idle_LazyChange
{
    [SerializeField] private BulletData _blowbackBulletData;
    [SerializeField] private float _maxDistance;               // 移動距離上限
    [SerializeField] private float _minAttackSpeed = 0.2f;     // ステート終了速度
    [SerializeField] private float _decelerationRate = 5.0f;   // 減速の強さ

    private Bullet _activeBullet;
    private float _initialSpeed;           // 初速度
    private float _currentKnockbackForce; // この吹き飛び自体の威力
    private Vector2 _velocity;
    private Vector2 _startPos;
    private Rigidbody2D _rb;

    public Blowback(BulletData bulletData, Rigidbody2D rigidbody2D, string lazyChange, float lazyChangeTime, bool isBlock = false)
        : base(lazyChange, lazyChangeTime, isBlock)
    {
        _blowbackBulletData = bulletData;
        _rb = rigidbody2D;
        _lazyChange = lazyChange;
        _lazyChangeTime = lazyChangeTime;
        _isBlock = isBlock;
    }

    public void PrepareBlowback(float force, Vector2 direction)
    {
        _currentKnockbackForce = force;
        _velocity = direction * force;
    }

    public void SetBulletData(BulletData bulletData)
    {
        _blowbackBulletData = bulletData;
    }

    public void SetMaxDistance(float maxDistance)
    {
        _maxDistance = maxDistance;
    }

    public void SetMinAttackSpeed(float minAttackSpeed)
    {
        _minAttackSpeed = minAttackSpeed;
    }

    public override void Enter(IState previousIState, UnitBase parent)
    {
        base.Enter(previousIState, parent);
        _startPos = parent.transform.position;
        _rb.linearVelocity = _velocity;
        _initialSpeed = _velocity.magnitude;

        // 吹き飛び攻撃用の弾生成
        if (_blowbackBulletData != null && _blowbackBulletData.prefab != null)
        {
            _activeBullet = GameObject.Instantiate(_blowbackBulletData.prefab, parent.transform.position, Quaternion.identity);
            if (_activeBullet is BlowbackBullet bBullet)
            {
                bBullet.SetBulletStatus(_blowbackBulletData, parent.AttackLayer);
                bBullet.SetupBlowback(parent, _currentKnockbackForce, _initialSpeed, _blowbackBulletData.originalstatus);
                bBullet.SetParent(parent);
                bBullet.Invoke();
                // bBullet.SetKnockbackForce(parent.statusManager.ReadValue(Status.knockbackMultiplier));
                Debug.Log($"BlowbackBullet : {_activeBullet.name}");
            }

            // 生成直後の向き設定
            UpdateBulletTransform(parent);
        }

        Debug.Log($"velocity : {_velocity}");
        Debug.Log($"rigidbody : {_rb}");
        Debug.Log($"rigidbody linerVelocity {_rb.linearVelocity}");
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, deltaTime * _decelerationRate);

        // 弾を追従させる
        UpdateBulletTransform(parent);

        // 速度がほぼ0なら遷移
        if (_rb.linearVelocity.magnitude < _minAttackSpeed)
        {
            _isBlock = false;
            parent.stateMachine.LazyChange(_lazyChange);
            return;
        }

        // 移動距離が最大をこえたら遷移
        if (Vector2.Distance(_startPos, parent.transform.position) >= _maxDistance)
        {
            _isBlock = false;
            parent.stateMachine.LazyChange(_lazyChange);
        }
    }

    public override void Exit(IState nextState, UnitBase parent)
    {
        _rb.linearVelocity = Vector2.zero;

        if (_activeBullet != null)
        {
            _activeBullet.NotifyDestoy();
            _activeBullet = null;
        }
    }

    // 弾を追従させ、移動方向を向かせる
    private void UpdateBulletTransform(UnitBase parent)
    {
        if (_activeBullet == null) return;

        _activeBullet.transform.position = parent.transform.position;

        Vector2 moveDir = _rb.linearVelocity;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            _activeBullet.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}

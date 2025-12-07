using System;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

public class DashAttack : ShootOnMoveBase
{
    [SerializeField] private bool _isStopInExit = false;
    [SerializeField] private float _startDashTime = 0.5f;
    [SerializeField] private float _turnSpeed = 10f;
    private float _dashSpeed;
    private float _time;
    private Vector2 _dashDirection;
    private bool _isBlock = true;
    private Bullet instantiatedBullet;

    
    public event Action OnCompleted;

    public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }


    // === Constractor ===
    public DashAttack(BulletData data, LayerMask targetLayer, bool isStopInExit = false) : base(data, targetLayer)
    {
        _data = data;
        _targetLayer = targetLayer;
        IsStopInExit = isStopInExit;
    }
    public DashAttack() : base() { }

    public void SetDashDirection(Vector2 direction)
    {
        _dashDirection = direction.normalized;
    }
    public void SetStateDashTime(float startDashTime)
    {
        _startDashTime = startDashTime;
    }
    public void SetBlock(bool isBlock)
    {
        _isBlock = isBlock;
    }

    // === Public ===
    public override void Enter(IState previousState, UnitBase parent)
    {
        base.Enter(previousState, parent);
        _dashSpeed = parent.statusManager.ReadValue(Status.DashSpeed);
        _dashDirection = parent.MoveDirection.normalized;
        _time = 0;
        _isBlock = true;
        _ = Shoot(parent);
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);

        _time += deltaTime;

        // 目標方向
        Vector2 goal = parent.MoveDirection.sqrMagnitude > 1e-6f ? parent.MoveDirection.normalized : _dashDirection;

        // 角度差を計算
        float angle = Vector2.SignedAngle(_dashDirection, goal);
        float maxDelta = _turnSpeed * deltaTime;
        float turn = Mathf.Clamp(angle, -maxDelta, maxDelta);
        _dashDirection = Rotate(_dashDirection, turn).normalized;

        // 移動（物理を用いる場合は MovePosition を推奨）
        Vector2 move = _dashDirection * _dashSpeed * deltaTime;
        rigidbody2D.MovePosition(rigidbody2D.position + move);

        // スタートダッシュ時間終了時、遷移可能
        if (_time >= _startDashTime)
        {
            _isBlock = false;
        }
    }

    public override void Exit(IState nextState, UnitBase parent)
    {
        base.Exit(nextState, parent);
        if (instantiatedBullet != null) UnityEngine.Object.Destroy(instantiatedBullet.gameObject);
        rigidbody2D.linearVelocity = Vector2.zero;
        OnCompleted?.Invoke();
    }

    // 状態変更をブロックする
    public override bool AllowChange(IState nextState, UnitBase parent)
    {
        if (_isBlock) return false;
        return base.AllowChange(nextState, parent);
    }

    private Vector2 Rotate(Vector2 v, float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        float ca = Mathf.Cos(rad), sa = Mathf.Sin(rad);
        return new Vector2(v.x * ca - v.y * sa, v.x * sa + v.y * ca);
    }

    protected override async UniTask Shoot(UnitBase parent)
    {
        var b = _data.prefab;
        if (b == null)
        {
            Debug.Log("Do not set bullet.");
        }
        // 弾の生成位置
        Vector3 spawnPos = _muzzle.transform.position + new Vector3(parent.AttackDirection.x, parent.AttackDirection.y) * _createPos;
        // 弾を生成
        instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity, _muzzle.transform);
        float angle = Mathf.Atan2(parent.AttackDirection.y, parent.AttackDirection.x) * Mathf.Rad2Deg;
        instantiatedBullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        InitBullet(instantiatedBullet, parent.AttackDirection);
        await base.Shoot(parent);
    }
}

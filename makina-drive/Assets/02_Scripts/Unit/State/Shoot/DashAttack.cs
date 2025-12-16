using System;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;

/// <summary>
/// ダッシュ攻撃ステート
/// </summary>
public class DashAttack : ShootOnMoveBase
{
    [SerializeField] private bool _isStopInExit = false;
    [SerializeField] private float _graceDashTime = 0.5f;

    [SerializeField, Min(0f)] protected float _accel = 60f;

    [Header("回転速度（度/秒）")]
    [SerializeField] private float rotateSpeed = 360f;
    private string _lazechange;  // ステート終了時の遷移先
    private float _dashSpeed;
    private float _time;
    private Vector2 _dashDirection;
    private bool _isBlock = true;

    public event Action OnCompleted;

    public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }


    // === Constractor ===
    public DashAttack(BulletData data, LayerMask targetLayer, bool isStopInExit = false, string lazeChange = null) : base(data, targetLayer)
    {
        _data = data;
        _targetLayer = targetLayer;
        IsStopInExit = isStopInExit;
        _lazechange = lazeChange;
    }
    public DashAttack() : base() { }

    public DashAttack SetAccel(float accel)
    {
        _accel = Mathf.Max(0f, accel);
        return this;
    }
    public void SetDashDirection(Vector2 direction)
    {
        _dashDirection = direction.normalized;
    }
    public void SetStateDashTime(float startDashTime)
    {
        _graceDashTime = startDashTime;
    }

    // === Public ===
    public override void Enter(IState previousState, UnitBase parent)
    {
        base.Enter(previousState, parent);
        _dashSpeed = parent.statusManager.ReadValue(Status.DashSpeed);
        _dashDirection = parent.Direction.normalized;
        _time = 0;
        _isBlock = true;
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        if (rigidbody2D == null) return;

        _time += deltaTime;

        // Debug.Log("time: " + _time);


        Debug.Log("_isBlock: " + _isBlock);
        if (_time >= _graceDashTime)
        {
            _isBlock = false;
            if (_lazechange != null)
                parent.stateMachine.LazyChange(_lazechange);
        }

        var maxSpeed = parent.statusManager.ReadValue(Status.DashSpeed);

        if (parent.MoveDirection.sqrMagnitude < 0.0001f)
        {
            // _dashDirection を使い続ける
        }
        else
        {
            _dashDirection = RotateTowards(
                _dashDirection,
                parent.Direction,
                rotateSpeed * Mathf.Deg2Rad * deltaTime
            );
        }
        var targetVel = _dashDirection * maxSpeed;

        var changePerSec = _accel;
        var maxDelta = changePerSec * Mathf.Max(deltaTime, 0f);

        rigidbody2D.linearVelocity = Vector2.MoveTowards(rigidbody2D.linearVelocity, targetVel, maxDelta);

        // 弾の位置を更新
        instantiatedBullet.transform.localPosition = new Vector3(parent.transform.position.x, parent.transform.position.y, parent.transform.position.z);

        // 弾の向きをダッシュ方向に合わせる
        instantiatedBullet.transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 1), _dashDirection);
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
        if (_isBlock)
        {
            Debug.Log("DashAttack: Change is blocked.");
            return false;
        }
        return base.AllowChange(nextState, parent);
    }

    /// <summary>
    /// from を to に向けて maxRadiansDelta だけ回転させる
    /// </summary>
    Vector2 RotateTowards(Vector2 from, Vector2 to, float maxRadiansDelta)
    {
        float angle = Vector2.SignedAngle(from, to);
        float angleRad = angle * Mathf.Deg2Rad;

        // 角度が小さければ to へスナップ
        if (Mathf.Abs(angleRad) <= maxRadiansDelta)
        {
            return to.normalized;
        }

        // 回転方向に maxRadiansDelta 分だけ回す
        float newAngleRad = Mathf.Clamp(angleRad, -maxRadiansDelta, maxRadiansDelta);
        float newAngleDeg = newAngleRad * Mathf.Rad2Deg;

        return Quaternion.Euler(0, 0, newAngleDeg) * from;
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
        instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
        instantiatedBullet.CanSelfMove = false;
        // float angle = Mathf.Atan2(parent.AttackDirection.y, parent.AttackDirection.x) * Mathf.Rad2Deg;
        InitBullet(instantiatedBullet, parent.AttackDirection);
        await base.Shoot(parent);
    }

    public Vector2 GetDashDirection()
    {
        return _dashDirection;
    }
}

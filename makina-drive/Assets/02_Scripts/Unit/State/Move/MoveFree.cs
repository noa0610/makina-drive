using UnityEngine;
using System;

/// <summary>
/// 自由移動ステート
/// </summary>
public class MoveFree : MoveStateBase
{
    [SerializeField] private bool _isStopInExit = false;

    [Header("Tuning")]
    [SerializeField, Min(0f)] protected float _accel = 60f;         // 入力ありのときの加速（速度ベクトルの変更量 [m/s^2]）
    [SerializeField, Min(0f)] protected float _decel = 30f;         // 入力なしのときの減速（速度ベクトルの変更量 [m/s^2]）
    [SerializeField, Range(0f, 0.1f)] protected float _deadZone = 0.001f; // 入力無視しきい値

    [SerializeField] protected float _delayTime;
    protected float _time = 0f;

    public event Action OnCompleted;
    protected string _lazyChange;

    public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }

    public MoveFree(bool isStopInExit = false) : base()
    {
        IsStopInExit = isStopInExit;
    }

    public override void Enter(IState previousIState, UnitBase parent)
    {
        base.Enter(previousIState, parent);
        _time = 0f;
        Action evt = null;
        evt = () =>
        {
            parent.stateMachine.LazyChange(_lazyChange);
            OnCompleted -= evt;
        };
        OnCompleted += evt;
    }


    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        if (rigidbody2D == null) return;

        var input = parent.MoveDirection;                   // 期待：(-1..1, -1..1)
        var hasInput = input.sqrMagnitude > (_deadZone * _deadZone);

        var maxSpeed = parent.statusManager.ReadValue(Status.Speed);
        var targetVel = hasInput ? input.normalized * maxSpeed : Vector2.zero;

        // 速度ベクトルをターゲットに滑らかに寄せる（ベクトル版 MoveTowards）
        var changePerSec = hasInput ? _accel : _decel;  // 入力時は加速、無入力時は減速
        var maxDelta = changePerSec * Mathf.Max(deltaTime, 0f);
        rigidbody2D.linearVelocity = Vector2.MoveTowards(rigidbody2D.linearVelocity, targetVel, maxDelta);

        _time += deltaTime;
        if (_time >= _delayTime)
        {
            OnCompleted?.Invoke();
            _time = 0f;
        }
    }


    public override void Exit(IState nextIState, UnitBase parent)
    {
        base.Enter(nextIState, parent);
        if (_isStopInExit && rigidbody2D != null)
            rigidbody2D.linearVelocity = Vector2.zero;
    }


    public MoveFree SetAccel(float accel)
    {
        _accel = Mathf.Max(0f, accel);
        return this;
    }

    public MoveFree SetDecel(float decel)
    {
        _decel = Mathf.Max(0f, decel);
        return this;
    }

    public MoveFree SetDeadZone(float dz)
    {
        _deadZone = Mathf.Clamp(dz, 0f, 0.1f);
        return this;
    }

    public void SetLazyChange(string lazyChange, float delayTime)
    {
        _lazyChange = lazyChange;
        _delayTime = delayTime;
    }
}

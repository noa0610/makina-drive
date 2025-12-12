using UnityEngine;

public class MoveInvincible : MoveStateBase
{
    [SerializeField] private bool _isStopInExit = false;

    private enum States
    {
        none,
        invincible,
        recovery
    }
    private States _currentState;

    [SerializeField] private float _dodgeAccel = 20f;    // 回避中の加速値
    [SerializeField] private float _dodgeMoveDecel = 20f;   // 回避中の減速値
    [SerializeField] private float _invincibleTime = 0.5f;   // 無敵時間
    [SerializeField] private float _dodgeRecoveryTime = 0.7f; // 無敵解除後の後隙回復時間
    private string _lazechange;  // ステート終了時の遷移先
    private float _time;
    private bool _isBlock = true;


    public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }

    public MoveInvincible(float invincibleTime, float recoveryTime, bool isStopInExit = false, string lazechange = null) : base()
    {
        _invincibleTime = invincibleTime;
        _dodgeRecoveryTime = recoveryTime;
        _isStopInExit = isStopInExit;
        _lazechange = lazechange;
    }
    public MoveInvincible() : base() { }


    public void SetAccel(float accel)
    {
        _dodgeAccel = accel;
    }
    public void SetDecel(float accel)
    {
        _dodgeMoveDecel = accel;
    }

    public override void Enter(IState previousState, UnitBase parent)
    {
        base.Enter(previousState, parent);
        _time = 0f;
        parent.SetInvincible(true);
        _currentState = States.invincible;
        _isBlock = true;

        if (rigidbody2D == null) return;
        var input = parent.Direction.normalized;
        var maxSpeed = parent.statusManager.ReadValue(Status.Speed) + _dodgeAccel;
        rigidbody2D.linearVelocity = input * maxSpeed;
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        _time += deltaTime;
        Debug.Log($"MoveInvincible Stay: Time={_time}");

        if (_currentState == States.invincible && _time >= _invincibleTime)
        {
            Debug.Log("invincible time over");
            _isBlock = false;
            parent.SetInvincible(false);
            _currentState = States.recovery;
        }
        else if (_currentState == States.recovery && _time >= _dodgeRecoveryTime)
        {
            Debug.Log("recovery time over");
            parent.stateMachine.LazyChange(_lazechange);
        }

        if (rigidbody2D == null) return;

        // 徐々に減速
        var currentVel = rigidbody2D.linearVelocity;
        var changePerSec = _dodgeMoveDecel;
        var maxDelta = changePerSec * Mathf.Max(deltaTime, 0f);
        rigidbody2D.linearVelocity = Vector2.MoveTowards(currentVel, Vector2.zero, maxDelta);
    }

    public override void Exit(IState nextIState, UnitBase parent)
    {
        base.Exit(nextIState, parent);

        // 念のため無敵状態解除処理
        parent.SetInvincible(false);

        if (_isStopInExit && rigidbody2D != null)
            rigidbody2D.linearVelocity = Vector2.zero;
    }

    // 状態変更をブロックする
    public override bool AllowChange(IState nextState, UnitBase parent)
    {
        Debug.Log("MoveInvincible: Checking AllowChange");
        if (_isBlock)
        {
            Debug.Log("MoveInvincible: State change blocked");
            return false;
        }
        return base.AllowChange(nextState, parent);
    }
}

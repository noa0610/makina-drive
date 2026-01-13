using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.VisualScripting;

/// <summary>
/// コンボステート
/// </summary>
[Serializable]
public class ShootCombo : ShootOnMoveBase
{
    protected enum ChangeState
    {
        Start,
        Buffering,
        Change
    }
    protected ChangeState _changeState;


    /* メモ：アニメーションに合わせて時間設定する */
    [SerializeField] protected float _inputReceptionTime;  // 入力受付開始時間
    [SerializeField] protected float _stateChangeTime;     // ステート終了時間
    [SerializeField] protected float _inputEndTime;        // 入力受付終了時間
    [SerializeField] protected float _attackStartTime;     // 攻撃を生成する時間

    [SerializeField] protected float _accel;               // 移動加速度


    protected string _lazechange;  // ステート終了時の遷移先
    protected string _comboChange; // コンボ遷移先

    protected string _blockThroughTag; // isBlockを無視するステートタグ

    protected bool _isAttackEnd;      // 攻撃終了
    protected bool _canInput;         // 入力許可
    protected bool _ComboStateChange; // コンボ先へ
    protected bool _isBlock;          // 遷移不可

    protected bool _isStopInExit = false;

    protected Vector2 _initialVelocity;  // ステート開始時初速度
    protected Vector2 _initialDirection; // ステート開始時移動方向

    public event Action OnCompleted;

    protected float _time;

    public bool IsStopInExit { get => _isStopInExit; set => _isStopInExit = value; }

    // === Constractor ===
    public ShootCombo(BulletData data, LayerMask targetLayer, string lazeChange, string comboChange,
                        float inputReceptionTIme, float inputEndTime, float stateChangeTime, float attackStartTime) : base(data, targetLayer)
    {
        _lazechange = lazeChange;
        _comboChange = comboChange;
        _inputReceptionTime = inputReceptionTIme;
        _inputEndTime = inputEndTime;
        _stateChangeTime = stateChangeTime;
        _attackStartTime = attackStartTime;
    }
    public ShootCombo(BulletData data, LayerMask targetLayer, string lazeChange, string comboChange = null) : base(data, targetLayer)
    {
        _lazechange = lazeChange;
        _comboChange = comboChange;
    }
    public ShootCombo() : base() { }
    
    // === Public ===
    /// <summary>
    /// 入力タイマーのセット
    /// </summary>
    /// <param name="inputReceptionTIme">入力受付時間</param>
    /// <param name="inputEndTime">入力終了時間（後隙）</param>
    /// <param name="stateChangeTime">入力受け取り時、実際にステート遷移する時間</param>
    /// <param name="attackStartTime">攻撃発生時間</param>
    public void SetTime(float inputReceptionTIme, float inputEndTime, float stateChangeTime, float attackStartTime)
    {
        _inputReceptionTime = inputReceptionTIme;
        _inputEndTime = inputEndTime;
        _stateChangeTime = stateChangeTime;
        _attackStartTime = attackStartTime;
    }
    public void SetInputReceptionTime(float time)
    {
        _inputReceptionTime = time;
    }
    public void SetStateChangeTime(float time)
    {
        _stateChangeTime = time;
    }
    public void SetInputEndTime(float time)
    {
        _inputEndTime = time;
    }
    public void SetAttackStartTime(float time)
    {
        _attackStartTime = time;
    }

    public void SetAccel(float accel)
    {
        _accel = accel;
    }

    public void SetCombo(bool ComboStateChange)
    {
        if (_canInput)
        {
            Debug.Log("入力受付前です。");
            return;
        }
        _ComboStateChange = ComboStateChange;
    }
    
    public void SetBlockThoroughTag(string blockThroughTag)
    {
        _blockThroughTag = blockThroughTag;
    }

    public void ResetAttack()
    {
        _isAttackEnd = false;
    }



    public override void Enter(IState previousIState, UnitBase parent)
    {
        _changeState = ChangeState.Start;
        _isAttackEnd = false;
        _canInput = false;
        _ComboStateChange = false;
        _time = 0;
        _isBlock = true;
        

        if (rigidbody2D != null && _accel > 0)
        {
            // 移動を一旦リセット
            rigidbody2D.linearVelocity = Vector2.zero;
            // 移動方向に一瞬加速する
            rigidbody2D.AddForce(parent.MoveDirection.normalized * _accel, ForceMode2D.Impulse);
            // 加速後の速度、方向を保存
            _initialVelocity = rigidbody2D.linearVelocity;
        }
        _initialDirection = parent.Direction.normalized;

    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        _time += deltaTime;

        // 攻撃生成処理
        if (_time >= _attackStartTime && !_isAttackEnd)
        {
            _ = Shoot(parent);
            _isAttackEnd = true;
        }

        if (instantiatedBullet != null)
        {
            // 弾を移動方向に_createPosの距離を空けて追従させる
            Vector3 targetPos = parent.transform.position + new Vector3(_initialDirection.x, _initialDirection.y) * _createPos;
            instantiatedBullet.transform.position = Vector3.Lerp(instantiatedBullet.transform.position, targetPos, 0.5f);
        }

        // 移動減速処理
        if (rigidbody2D != null && _stateChangeTime > 0)
        {
            // 経過割合 (0.0 ～ 1.0)
            float ratio = _time / _stateChangeTime;

            // 開始時の速度から 0 に向かって線形補間（Lerp）float ratio = _time / _stateChangeTime;
            float easedRatio = 1f - Mathf.Pow(1f - ratio, 2); // EaseOutQuad
            rigidbody2D.linearVelocity = Vector2.Lerp(_initialVelocity, Vector2.zero, easedRatio);
        }

        // ステート遷移処理
        if (_changeState == ChangeState.Start)
        {
            if (_time >= _inputReceptionTime)
            {
                Debug.Log("InputReceptionTime");
                // 入力を許可
                _canInput = true;
                _isBlock = false;
                _changeState = ChangeState.Buffering;
            }
        }
        else if (_changeState == ChangeState.Buffering)
        {
            if (_time >= _stateChangeTime)
            {
                Debug.Log("StateChangeTime");
                // 遷移可能な状態に移行
                _changeState = ChangeState.Change;
            }
        }
        else if (_changeState == ChangeState.Change)
        {
            // コンボを繋げる遷移へ移動
            if (_ComboStateChange)
            {
                if (_comboChange == null) return;
                parent.stateMachine.LazyChange(_comboChange);
            }

            // 終了時間の遷移へ
            if (_time >= _inputEndTime)
            {
                Debug.Log("InputEndTime");
                _canInput = false;
                _ComboStateChange = false;
                parent.stateMachine.LazyChange(_lazechange);
            }
        }
    }

    public override void Exit(IState nextState, UnitBase parent)
    {
        base.Exit(nextState, parent);
        if (instantiatedBullet != null) UnityEngine.Object.Destroy(instantiatedBullet.gameObject);
        if (_isStopInExit && rigidbody2D != null)
            rigidbody2D.linearVelocity = Vector2.zero;
        OnCompleted?.Invoke();
    }

    public override bool AllowChange(IState nextState, UnitBase parent)
    {
        // 遷移先の情報を取得
        var nextStateInfo = parent.stateMachine.GetStateInfo(nextState);

        if (nextStateInfo.HasTag(_blockThroughTag))
        {
            return base.AllowChange(nextState, parent);
        }

        if (_isBlock)
        {
            return false;
        }
        return base.AllowChange(nextState, parent);
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
        InitBullet(instantiatedBullet, parent.AttackDirection, parent);
        await base.Shoot(parent);
    }    

    protected override void InitBullet(Bullet bullet, Vector3 dict, UnitBase parent)
    {
        base.InitBullet(bullet, dict, parent);
        bullet.isParentDeadBulleDestroy = true;
    }
}

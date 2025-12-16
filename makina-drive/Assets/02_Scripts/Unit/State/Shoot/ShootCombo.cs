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
    private enum ChangeState
    {
        Start,
        Buffering,
        Change
    }
    private ChangeState _changeState;


    /* メモ：アニメーションに合わせて時間設定する */
    [SerializeField] private float _inputReceptionTime;  // 入力受付開始時間
    [SerializeField] private float _stateChangeTime;     // ステート終了時間
    [SerializeField] private float _inputEndTime;        // 入力受付終了時間
    [SerializeField] private float _attackStartTime;     // 攻撃を生成する時間

    [SerializeField] private float _accel;               // 移動加速度


    private string _lazechange;  // ステート終了時の遷移先
    private string _comboChange; // コンボ遷移先

    private bool _isAttackEnd;      // 攻撃終了
    private bool _canInput;         // 入力許可
    private bool _ComboStateChange; // コンボ先へ

    private Vector2 _initialVelocity; // ステート開始時初速度

    public event Action OnCompleted;

    private float _time;

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



    public override void Enter(IState previousIState, UnitBase parent)
    {
        _changeState = ChangeState.Start;
        _isAttackEnd = false;
        _canInput = false;
        _ComboStateChange = false;
        _time = 0;

        if (rigidbody2D != null && _accel > 0)
        {
            // 移動を一旦リセット
            rigidbody2D.linearVelocity = Vector2.zero;
            // 移動方向に一瞬加速する
            rigidbody2D.AddForce(parent.MoveDirection.normalized * _accel, ForceMode2D.Impulse);
            // 加速後の速度を保存
            _initialVelocity = rigidbody2D.linearVelocity;
        }

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
        OnCompleted?.Invoke();
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
        instantiatedBullet.CanSelfMove = false;
        // float angle = Mathf.Atan2(parent.AttackDirection.y, parent.AttackDirection.x) * Mathf.Rad2Deg;
        // instantiatedBullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        InitBullet(instantiatedBullet, parent.AttackDirection);
        await base.Shoot(parent);
    }

    // === Public ===
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

    // 攻撃済みであれば再度攻撃を行う
    public void ResetAttack()
    {
        _isAttackEnd = false;
    }
}

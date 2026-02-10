using System;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator)), Serializable]
public abstract class UnitBase : MonoBehaviour, IUnit
{
    #region === Inspector References ===
    [SerializeField] protected UnitStatusData _status;
    [SerializeField] protected LayerMask _attackLayer;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected GameObject _muzzle;
    #endregion

    [SerializeField] public static bool _isPlaying = true;

    #region === Components & Managers ===
    protected IStateMachine _stateMachine;
    private StatusManager _statusManager;
    private Rigidbody2D _body2D;
    protected RecoveryStatus _recoveryStatus;
    #endregion


    #region === Properties ===
    public IStateMachine stateMachine => _stateMachine;
    public StatusManager statusManager => _statusManager;
    public RecoveryStatus recoveryStatus => _recoveryStatus;
    public UnitStatusData UnitStatusData => _status;
    public Rigidbody2D Rigidbody2D => _body2D;
    public GameObject Muzzle => _muzzle;
    public Transform Transform => transform;
    public LayerMask AttackLayer => _attackLayer;
    public Animator Animator
    {
        get => _animator;
        set => _animator = value;
    }
    public bool IsInvincible { get; set; }
    public bool IsArrivals { get; set; } = true;
    public bool IsRecovery { get; set; } = true;
    public bool IsClearTarget { get; set; } = false;
    private int _hitStopCount = 0;
    private float _originalAnimatorSpeed = 1f;
    public static event Action<UnitBase> OnAnyUnitDeath; // ユニット死亡イベント
    public event Action<UnitBase> OnUnitDeath;           // 個別の死亡イベント
    private bool IsLazyDead = false;
    private float lazyDeadTime = 0;
    public float DropExp { get; set; } // 敵が保持する経験値量
    #endregion

    #region === Reactive & Direction ===
    [SerializeField] private Vector2 _Dir = Vector2.zero;
    [SerializeField] private Vector2 _moveDir = Vector2.zero;
    [SerializeField] private Vector2 _shootDir = Vector2.right;
    private ReactiveProperty<Vector2> _reactiveDirection = new(new(1, 0));
    public IObservable<Vector2> ReactiveDirection => _reactiveDirection;
    public Vector2 Direction { get => _Dir; set => _Dir = value; }
    public Vector2 MoveDirection { get => _moveDir; set => _moveDir = value; }
    public Vector2 AttackDirection { get => _shootDir; set => _shootDir = value; }

    public Vector2 knockbackDirection { get; private set; } = Vector2.right; // 攻撃を受けて押し出される方向
    public float KnockbackForce;

    public enum StartDirection
    {
        Left,
        Right
    }
    [SerializeField] private StartDirection _StartDirection = StartDirection.Left;
    #endregion


#if UNITY_EDITOR
    #region === Debug ===
    [Header("Debug")]
    [SerializeField] private bool _enableVisuableInvincible;
    [SerializeField] private string _currentState;

    public virtual bool ShoudBeLogging => false;
    #endregion
#endif


    #region === Initialization ===
    protected virtual string StartState => "idle";
    protected virtual void BeforeAwake() { }
    protected virtual void AfterAwake() { }

    protected virtual void BeforeRegisterStats() { }
    protected abstract void RegisterStats();


    protected void Awake()
    {
        BeforeAwake();

        if (_animator == null)
            _animator = GetComponent<Animator>();
        _body2D = GetComponent<Rigidbody2D>();
        _body2D.gravityScale = 0;

        // UnitManager.instance.AddUnit(this);
        _stateMachine = new StateMachine(this, new AnimatorAnimationDriver(_animator));
        _statusManager = new StatusManager();

        BeforeRegisterStats();

        _statusManager.Initialize(_status);
        RegisterStats();
        InitDirection();

        // ひとまず固定値
        const float RECOVERY_RATE = 10f;
        const float RECOVERY_DELAY = 1f;
        _recoveryStatus = new RecoveryStatus(statusManager, Status.Stamina, RECOVERY_RATE, RECOVERY_DELAY);

#if UNITY_EDITOR
        // ログ設定切り替え可
        _stateMachine.Awake(StartState, ShoudBeLogging);
#else
        // ログ無し
        _stateMachine.Awake(StartState, false);
#endif
        AfterAwake();
    }

    private void InitDirection()
    {
        switch (_StartDirection)
        {
            case StartDirection.Left:
                MoveDirection = Vector2.left;
                Direction = Vector2.left;

                break;
            case StartDirection.Right:
                MoveDirection = Vector2.right;
                Direction = Vector2.right;
                break;
        }

        var scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (Direction.x >= 0f ? 1f : -1f);
        transform.localScale = scale;
    }

    protected virtual void Start()
    {

    }
    #endregion

    #region === Update Cycle ===
    protected virtual void BeforeUpdate() { }
    protected virtual void OnUpdate() { }
    protected virtual void AfterUpdate() { }
    protected virtual void BeforeFixedUpdate() { }
    protected virtual void AfterFixedUpdate() { }

    protected void Update()
    {
        BeforeUpdate();
        if (!_isPlaying) return;

        OnUpdate();

        var dt = Time.deltaTime;
        // _stateMachine.UpdateMachine(dt);

        if (IsRecovery)
        {
            // スタミナの自動回復
            _recoveryStatus?.Tick(dt);
        }

        // 死亡タイマー
        if (IsLazyDead && dt >= lazyDeadTime)
        {
            OnDeath();
        }

        AfterUpdate();
    }

    protected virtual void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        _stateMachine.UpdateMachine(dt);

#if UNITY_EDITOR
        _currentState = _stateMachine.CurrentState.key;
#endif
        BeforeFixedUpdate();
        if (!_isPlaying) return;
        AfterFixedUpdate();
    }
    #endregion

    #region === Status & Damage ===
    protected virtual bool BeforeTakeDamage(IUnit from, ref float damage) => true;
    protected virtual void OnTakeDamage(IUnit from, float damage, Vector2 pushdir, float knockbackForce = 0) { }
    public void TakeDamage(IUnit from, float damage, Vector2 pushdir, float knockbackForce = 0)
    {
        if (!_isPlaying || !IsArrivals) return;
        if (!BeforeTakeDamage(from, ref damage)) return;

        if (_statusManager.TakeDamage(damage))
            OnDeath();

        OnTakeDamage(from, damage, pushdir, knockbackForce);
    }

    // ヒットストップ処理
    public async UniTaskVoid HitStop(float duration)
    {
        if (duration <= 0 || _animator == null) return;

        _hitStopCount++;

        if (_hitStopCount == 1)
        {
            _originalAnimatorSpeed = _animator.speed > 0 ? _animator.speed : 1f;
        }

        // 動きを止める
        _animator.speed = 0;
        Vector3 currentVelocity = Rigidbody2D.linearVelocity;
        Rigidbody2D.linearVelocity = Vector2.zero;

        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: this.GetCancellationTokenOnDestroy());

        _hitStopCount--;

        // ヒットストップ要求が終わったら速度を戻す
        if (_hitStopCount <= 0)
        {
            _hitStopCount = 0;

            if (_animator != null) _animator.speed = _originalAnimatorSpeed;
        }

    }


    public virtual void OnDeath()
    {
        // 死亡通知を飛ばす
        OnUnitDeath?.Invoke(this);
        OnAnyUnitDeath?.Invoke(this);

        if (_status.unitName != null)
        {
            Debug.Log($"{_status.unitName}が死亡した");
        }
        else
        {
            Debug.Log($"{_status.name}が死亡した");
        }
    }

    // 死亡タイマーをセット
    public void SetLazyDeath(float deadTime)
    {
        IsLazyDead = true;
        lazyDeadTime = deadTime;
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    public virtual void GainExp(float amount) { }

    // 敵ウェーブ生成専用
    public void ApplyWaveStatus(float multiplier, List<Status> targets = null)
    {
        if (statusManager == null || targets == null || targets.Count == 0) return;

        statusManager.ApplyStatusMultiplier(targets, multiplier);
        // statusManager.TakeHeal(statusManager.ReadValue(Status.MaxHP));
    }
    public void ApplyWaveStatus(List<StatusOverride> overrides)
    {
        if(statusManager == null || overrides == null || overrides.Count == 0) return;

        statusManager.ApplyStatusOverride(overrides);
        // statusManager.TakeHeal(statusManager.ReadValue(Status.MaxHP));
    }

    #endregion

    #region === Pause & Play ===
    public virtual void Pause() { }
    public virtual void Play() { }
    #endregion

    public void PlaySE(string bgmName, float volume = 1f)
    {
        if (SoundManager.instance != null && bgmName != null)
        {
            SoundManager.instance.PlaySE(bgmName, volume);
        }
    }
}

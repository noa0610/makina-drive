using System;
using UnityEngine;
using UniRx;

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
    #endregion


    #region === Properties ===
    public IStateMachine stateMachine => _stateMachine;
    public StatusManager statusManager => _statusManager;
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
    #endregion

    #region === Reactive & Direction ===
    [SerializeField] private Vector2 _moveDir = Vector2.zero;
    [SerializeField] private Vector2 _shootDir = Vector2.right;
    private ReactiveProperty<Vector2> _reactiveDirection = new(new(1, 0));
    public IObservable<Vector2> ReactiveDirection => _reactiveDirection;
    public Vector2 Direction
    {
        get => _reactiveDirection.Value;
        set => _reactiveDirection.Value = value;
    }
    public Vector2 MoveDirection { get => _moveDir; set => _moveDir = value; }
    public Vector2 AttackDirection { get => _shootDir; set => _shootDir = value; }
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
        _statusManager = new StatusManager();
        _body2D = GetComponent<Rigidbody2D>();
        _body2D.gravityScale = 0;

        // UnitManager.instance.AddUnit(this);
        _stateMachine = new StateMachine(this, new AnimatorAnimationDriver(_animator));
        _statusManager = new StatusManager();

        BeforeRegisterStats();
        _statusManager.Initialize(_status);
        RegisterStats();

#if UNITY_EDITOR
        // ログ設定切り替え可
        _stateMachine.Awake(StartState, ShoudBeLogging);
#else
        // ログ無し
        _stateMachine.Awake(StartState, false);
#endif
        AfterAwake();
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
        _stateMachine.UpdateMachine(dt);

        AfterUpdate();
    }

    protected virtual void FixedUpdate()
    {
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
    protected virtual void OnTakeDamage(IUnit from, float damage) { }
    public void TakeDamage(IUnit from, float damage)
    {
        if (!_isPlaying || !IsArrivals) return;
        if (!BeforeTakeDamage(from, ref damage)) return;

        if (_statusManager.TakeDamage(damage))
            OnDeath();

        OnTakeDamage(from, damage);
    }

    public virtual void OnDeath()
    {
        Debug.Log($"{_status.name}が死亡した");
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

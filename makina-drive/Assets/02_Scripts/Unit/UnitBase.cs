using System;
using UnityEngine;

[RequireComponent(typeof(Animator)), Serializable]
public abstract class UnitBase : MonoBehaviour
{
    [SerializeField] protected UnitStatusData _status;
    public float MaxHP;
    public float currentHP;
    public StatusManager statusManager;
    protected StateMachine _stateMachine;
    public Vector2 direction;
    protected Animator _Animator;
    [SerializeField] public bool isPlaying = true; // ゲームプレイ中か否か

#if UNITY_EDITOR
    // エディタからの監視用
    [Header("Debug")]
    [SerializeField] private string _currentState;
# endif

    public UnitStatusData UnitStatusData => _status;
    public StateMachine StateMachine => _stateMachine;
    public Transform Transform => transform;


    // 向き
    public Vector2 Direction
    {
        get { return direction; }
        set { direction = value; }
    }
    // 移動方向（入力依存）
    public Vector2 MoveDirection { get; set; }
    public Vector2 AttackDirection { get; set; }
    public Animator Animator
    {
        get { return _Animator; }
        set { _Animator = value; }
    }
    public bool IsInvincible { get; set; }

    protected abstract void RegisterStats();

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            currentHP = 0;
            OnDead();
        }
    }

    public virtual void OnDead()
    {
        Debug.Log($"{_status.name}が死亡した");
    }

    public virtual void Pause()
    {

    }

    public virtual void Play()
    {

    }

    protected void Awake()
    {
        BeforeAwake();
        _Animator = GetComponent<Animator>();
        statusManager = new StatusManager();
        MaxHP = _status.maxHp;
        currentHP = _status.hp;
        RegisterStats();
        AfterAwake();
    }
    protected virtual void BeforeAwake()
    {

    }
    protected virtual void AfterAwake()
    {

    }

    protected virtual void Start()
    {

    }

    protected void Update()
    {
        BeforeUpdate();
        if (!isPlaying) return;
        AfterUpdate();
    }
    // isPlayingの状態に関わらず呼ばれる
    protected virtual void BeforeUpdate()
    {

    }
    // isPlayingがtrueのときのみ呼ばれる
    protected virtual void AfterUpdate()
    {

    }

    protected void FixedUpdate()
    {
        BeforeFixedUpdate();
        if (!isPlaying) return;
        AfterFixedUpdate();
    }
    // isPlayingの状態に関わらず呼ばれる
    protected virtual void BeforeFixedUpdate()
    {

    }
    // isPlayingがtrueのときのみ呼ばれる
    protected virtual void AfterFixedUpdate()
    {

    }
}

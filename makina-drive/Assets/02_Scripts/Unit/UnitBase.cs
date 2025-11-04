using System;
using UnityEngine;

[RequireComponent(typeof(Animator)), Serializable]
public class UnitBase : MonoBehaviour
{
    [SerializeField] protected UnitStatusData _status;
    public StatusManager statusManager;
    public Vector2 direction;
    protected Animator _Animator;
    public Transform Transform => transform;
    [SerializeField] public bool isPlaying = true; // ゲームプレイ中か否か
    public bool IsInvincible;


    public Vector2 Direction
    {
        get { return direction; }
        set { direction = value; }
    }
    public Vector2 MoveDirection { get; set; }
    public Animator Animator
    {
        get { return _Animator; }
        set { _Animator = value; }
    }

    public void TakeDamage(float damage)
    {

    }

    public virtual void OnDead()
    {

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

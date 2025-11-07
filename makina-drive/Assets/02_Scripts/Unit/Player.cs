using JetBrains.Annotations;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : UnitBase
{
    [Header("固有設定")]
    [SerializeField] private float MoveSpeed = 7f;
    private Rigidbody2D rb;
    private enum State
    {
        none,
        idle,
        move,
        dodge,
        N_attack1,
        N_attack2,
        N_attack3,
        S_attack,
        dlivedash,
        jumpstart,
        jumpfallAim,
        fallAttack,
        stan,
        dead
    }

    private enum Trigger
    {
        none
    }

    private Dictionary<State, string> _states;

    // ステート登録
    protected override void RegisterStats()
    {
        
    }

    protected override void Start()
    {

    }

    protected override void BeforeAwake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public void OnMove(InputValue value)
    {
        var d = value.Get<Vector2>();

        if (d != Vector2.zero) Direction = d.normalized;

        MoveDirection = d.normalized;
        AttackDirection = d.normalized;

        if (d == Vector2.zero)
        {
            // 移動停止処理
            return;
        }
    }

    protected override void BeforeFixedUpdate()
    {
        rb.linearVelocity = MoveDirection * MoveSpeed;
    }
}

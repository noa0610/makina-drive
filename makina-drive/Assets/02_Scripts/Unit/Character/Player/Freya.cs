using JetBrains.Annotations;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public partial class Freya : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
    private Rigidbody2D rb;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("攻撃")]
    [SerializeField] private BulletData bulletData;

    protected override void Start()
    {

    }

    protected override void BeforeAwake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }


    protected override void BeforeFixedUpdate()
    {
        TurnAround();
    }

    private void TurnAround()
    {
        if (Direction.x != 0)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
            transform.localScale = scale;
        }

    }

    private bool IsMatchingState(States state)
    {
        return _stateMachine.CurrentState.key == _stateNames[state];
    }
}

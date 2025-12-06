using JetBrains.Annotations;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;

[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public partial class Freya : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
    private Rigidbody2D rb;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("攻撃共通")]
    [SerializeField] private float _createPos = 4f;

    [Header("通常攻撃１")]
    [SerializeField] private BulletData N1_bulletData;
    [SerializeField] private float N1_inputReceptionTime = 0.2f;
    [SerializeField] private float N1_stateChangeTime = 0.7f;
    [SerializeField] private float N1_inputEndTime = 0.9f;
    [SerializeField] private float N1_attackStartTime = 0.3f;

    [Header("通常攻撃２")]
    [SerializeField] private BulletData N2_bulletData;
    [SerializeField] private float N2_inputReceptionTime = 0.2f;
    [SerializeField] private float N2_stateChangeTime = 0.7f;
    [SerializeField] private float N2_inputEndTime = 0.8f;
    [SerializeField] private float N2_attackStartTime = 0.3f;

    [Header("通常攻撃３")]
    [SerializeField] private BulletData N3_bulletData;
    [SerializeField] private float N3_inputReceptionTime = 0.2f;
    [SerializeField] private float N3_stateChangeTime = 0.8f;
    [SerializeField] private float N3_inputEndTime = 1.2f;
    [SerializeField] private float N3_attackStartTime = 0.4f;

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

    /// <summary>
    ///  振り向き
    /// </summary>
    private void TurnAround()
    {
        if (Direction.x != 0)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (Direction.x > 0 ? 1 : -1);
            transform.localScale = scale;
        }

    }

    /// <summary>
    /// 現在ステートの判別
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool IsMatchingState(States state)
    {
        return _stateMachine.CurrentState.key == _stateNames[state];
    }
}

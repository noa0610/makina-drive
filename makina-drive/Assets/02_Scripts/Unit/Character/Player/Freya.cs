using JetBrains.Annotations;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Linq;
using Unity.VisualScripting;

[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public partial class Freya : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
    private Rigidbody2D rb;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("回避")]
    [SerializeField] private float _dodgeAccel = 10f;
    [SerializeField] private float _dodgeDecel = 10f;
    [SerializeField] private float _invincibleTime = 0.5f;   // 無敵時間
    [SerializeField] private float _dodgeRecoveryTime = 0.7f; // 無敵解除後の後隙回復時間
    [SerializeField] private float _dodgeStaminaLostAmount = 20;

    [Header("ドライブダッシュ")]
    [SerializeField] private BulletData Dash_bulletData;
    [SerializeField] private float _dashStaminaFrameLostAmount = 0.05f;


    [Header("攻撃共通")]
    [SerializeField] private float _createPos = 4f;
    [SerializeField] private float _attackAccel = 5f;


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


    [Header("ダッシュ通常攻撃１")]
    [SerializeField] private BulletData DashN1_bulletData;
    [SerializeField] private float DashN1_inputReceptionTime = 0.1f;
    [SerializeField] private float DashN1_stateChangeTime = 0.6f;
    [SerializeField] private float DashN1_inputEndTime = 0.8f;
    [SerializeField] private float DashN1_attackStartTime = 0.2f;

    [Header("ダッシュ通常攻撃２")]
    [SerializeField] private BulletData DashN2_bulletData;
    [SerializeField] private float DashN2_inputReceptionTime = 0.2f;
    [SerializeField] private float DashN2_stateChangeTime = 0.6f;
    [SerializeField] private float DashN2_inputEndTime = 0.9f;
    [SerializeField] private float DashN2_attackStartTime = 0.2f;

    [Header("ダッシュ通常攻撃３")]
    [SerializeField] private BulletData DashN3_bulletData;
    [SerializeField] private float DashN3_inputReceptionTime = 0.3f;
    [SerializeField] private float DashN3_stateChangeTime = 0.6f;
    [SerializeField] private float DashN3_inputEndTime = 1f;
    [SerializeField] private float DashN3_attackStartTime = 0.4f;

    // TODO チャージ攻撃は斬撃を飛ばす


    [Header("ジャンプ開始")]
    [SerializeField] private float _jumpStartTime = 1f;
    [SerializeField] private float _jumpStaminaLostAmount = 80f;

    [Header("落下狙い")]
    [SerializeField] private float _fallAimAccel = 40f;
    [SerializeField] private float _fallAimDecel = 30f;

    [Header("落下")]
    [SerializeField] private float _fallTime = 0.12f;

    [Header("落下攻撃")]
    [SerializeField] private BulletData FallAttack_bulletData;
    [SerializeField] private float _fallAttackTime = 1.0f;


    private Vector2 _dashDirection = Vector2.right;
    private bool _inputDash = false;

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

        // Debug.Log("inputDash: " + _inputDash);

        if (IsMatchingState(States.drivedash))
        {
            if (_inputDash == false || statusManager.ReadValue(Status.Stamina) <= 0)
            {
                IsRecovery = true;
                stateMachine.ChangeState(Triggers.dashCancel);
            }
            statusManager.AddValue(Status.Stamina, -_dashStaminaFrameLostAmount);
            _dashDirection = drivedash.GetDashDirection();
        }
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

    public override void OnDeath()
    {
        base.OnDeath();
    }


    // --- シーンビューに方向を描画 ---
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_dashDirection);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Direction);
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

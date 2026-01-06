using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework;
using UniRx;

/// <summary>
/// 近づいて攻撃を行う敵ユニット
/// </summary>
public partial class Enemy_Normal : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();

    [Header("デバッグ")]
    [SerializeField] private UnitBase _targetUnitForDebug;
    [SerializeField] private bool _findPlayerOnStart = false;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("攻撃")]
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private float _attackCreatePos = 1.5f;
    [SerializeField] private float _stateChangeTime = 2f;
    [SerializeField] private float _attackTime = 1f;
    [SerializeField] private BulletData _attackBulletData;

    [Header("スタン")]
    [SerializeField] private bool _ignoreStan = false; // スタン状態を無視
    [SerializeField] private float _stanTime = 0.5f;

    [Header("吹き飛ばし")]
    [SerializeField] private float _blowbackMaxDistance = 30f;

    [Header("経験値アイテム")]
    [SerializeField] private ExpItem _expItem;

    [Header("回復アイテム")]
    [SerializeField] private HealItem _healItem;
    [SerializeField] private float _dropProbability = 0.075f;


    [Header("SE")]
    [SerializeField] private VisualInfo _AttackSE;
    [SerializeField] private VisualInfo _DamageSE;
    [SerializeField] private VisualInfo _BlowbackSE;
    [SerializeField] private VisualInfo _DeadSE;

    private UnitBase _targetUnit;
    private Transform _targetTransform;

    protected override void Start()
    {
        base.Start();

        if (_findPlayerOnStart)
        {
            if (_targetUnitForDebug != null)
            {
                SetTarget(_targetUnitForDebug);
            }
            else
            {
                var player = GameObject.FindWithTag("Player")?.GetComponent<UnitBase>();
                if (player != null)
                {
                    SetTarget(player);
                }
                else
                {
                    Debug.LogWarning("Playerが見つかりません。");
                }
            }
        }
    }

    public void SetTarget(UnitBase target)
    {
        _targetUnit = target;
        _targetTransform = target.Transform;
    }

    protected override void OnTakeDamage(IUnit from, float damage, Vector2 pushdir, float knockbackForce)
    {
        base.OnTakeDamage(from, damage, pushdir, knockbackForce);

        if (knockbackForce > 0)
        {
            blowback.SetVelocity(knockbackForce, pushdir);
            PlaySE(_BlowbackSE.SEName, _BlowbackSE.Volume);
            _stateMachine.ChangeState(Triggers.toBlowback);
        }

        if (damage > 0 && !_ignoreStan)
        {
            PlaySE(_DamageSE.SEName, _DamageSE.Volume);
            _stateMachine.ChangeState(Triggers.toStan);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();

        if (_targetUnit != null)
        {
            // 経験値アイテムドロップ
            if (_expItem != null)
            {
                float finalExp = UnitStatusData.baseExp;

                var expObj = Instantiate(_expItem, transform.position, Quaternion.identity);
                expObj.GetComponent<ExpItem>();
                expObj.Setup(_targetUnit);
                expObj.SetExp(finalExp);
            }

            // 回復アイテムドロップ抽選
            if (_healItem != null && UnityEngine.Random.value <= _dropProbability)
            {
                var healObj = Instantiate(_expItem, transform.position, Quaternion.identity);
                healObj.GetComponent<ExpItem>();
            }
        }

        PlaySE(_DeadSE.SEName, _DeadSE.Volume);
        UnitManager.instance.RemoveUnit(this);
        Destroy(this.gameObject);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (IsMatchingState(States.idle))
        {
            stateMachine.ChangeState(Triggers.toChase);
        }

        if (_targetTransform != null)
        {
            var dir = (_targetTransform.position - Transform.position).normalized;
            Direction = dir;
            MoveDirection = dir;
            AttackDirection = dir;
            TurnAround();

            if (IsMatchingState(States.chase))
            {
                var distance = Vector2.Distance(_targetTransform.position, Transform.position);
                if (distance <= _attackRange)
                {
                    _stateMachine.ChangeState(Triggers.toAttack);
                }
            }
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

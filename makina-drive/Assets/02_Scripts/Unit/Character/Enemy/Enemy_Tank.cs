using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 近づいて突進攻撃を行う敵ユニット
/// </summary>
public partial class Enemy_Tank : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();

    [Header("デバッグ")]
    [SerializeField] private UnitBase _targetUnitForDebug;
    [SerializeField] private bool _findPlayerOnStart = false;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("タックル")]
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private float _attackCreatePos = 1.5f;
    [SerializeField] private float _stateChangeTime = 2f;
    [SerializeField] private float _attackIntervalTime = 1f;
    [SerializeField] private BulletData _attackBulletData;

    [Header("スタン")]
    [SerializeField] private bool _ignoreStan = false; // スタン状態を無視
    [SerializeField] private float _stanTime = 0.5f;

    [Header("吹き飛ばし")]
    [SerializeField] private float _blowbackMaxDistance = 30f;

    private UnitBase _targetUnit;
    private Transform _targetTransform;
    private float _timer;

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
            _stateMachine.ChangeState(Triggers.toBlowback);
        }

        if (damage > 0 && !_ignoreStan)
        {
            _stateMachine.ChangeState(Triggers.toStan);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        UnitManager.instance.RemoveUnit(this);
        Destroy(this.gameObject);
    }

    protected override void AfterUpdate()
    {
        base.AfterUpdate();
        if (IsMatchingState(States.chase))
        {
            _timer += Time.deltaTime;
        }
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
            if (IsMatchingState(States.chase))
            {
                // プレイヤー方向振り向き
                var dir = (_targetTransform.position - Transform.position).normalized;
                Direction = dir;
                MoveDirection = dir;
                AttackDirection = dir;
                TurnAround();

                chase.SetAccel(_accel);

                // 攻撃遷移距離の判定
                var distance = Vector2.Distance(_targetTransform.position, Transform.position);
                if (distance <= _attackRange)
                {
                    if (_timer >= _attackIntervalTime)
                    {
                        _stateMachine.ChangeState(Triggers.toAttack);
                    }
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

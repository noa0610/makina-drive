using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 遠距離攻撃を行う敵ユニット
/// </summary>
public partial class Enemy_Sniper : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();
    private Rigidbody2D rb;

    [Header("デバッグ")]
    [SerializeField] private UnitBase _targetUnitForDebug;
    [SerializeField] private bool _findPlayerOnStart = false;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;

    [Header("射撃")]
    [SerializeField] private float _ShootRange = 15f;
    [SerializeField] private float _ShootCreatePos = 1.5f;
    [SerializeField] private float _ShootStateChangeTime = 2f;
    [SerializeField] private BulletData _ShootBulletData;

    [Header("スタン")]
    [SerializeField] private float _stanTime = 0.5f;
    
    [Header("吹き飛ばし")]
    [SerializeField] private float _blowbackMaxDistance = 30f;

    private UnitBase _targetUnit;
    private Transform _targetTransform;

    private float _shootTimer = 0;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();

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

        // 吹き飛ばし状態移行
        if(knockbackForce > 0)
        {
            blowback.SetVelocity(knockbackForce, pushdir);
            _stateMachine.ChangeState(Triggers.toBlowback);
        }

        // スタン状態移行
        if (damage > 0)
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
        _shootTimer += Time.deltaTime;
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
                if (distance <= _ShootRange && _shootTimer >= _ShootStateChangeTime)
                {
                    _stateMachine.ChangeState(Triggers.toShoot);
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

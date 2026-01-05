using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 遠距離攻撃を行う敵ユニット
/// </summary>
public partial class Enemy_Sniper : UnitBase
{
    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();

    [Header("デバッグ")]
    [SerializeField] private UnitBase _targetUnitForDebug;
    [SerializeField] private bool _findPlayerOnStart = false;

    [Header("移動")]
    [SerializeField] private float _accel = 30f;
    [SerializeField] private float _decel = 20f;
    [SerializeField] private float _MoveStopRange = 5f;

    [Header("射撃")]
    [SerializeField] private float _ShootRange = 15f;
    [SerializeField] private float _ShootCreatePos = 1.5f;
    [SerializeField] private float _ShootStateChangeTime = 2f;
    [SerializeField] private BulletData _ShootBulletData;

    [Header("スタン")]
    [SerializeField] private bool _ignoreStan = false; // スタン状態を無視
    [SerializeField] private float _stanTime = 0.5f;

    [Header("吹き飛ばし")]
    [SerializeField] private float _blowbackMaxDistance = 30f;

    [Header("経験値アイテム")]
    [SerializeField] private ExpItem _expItem;

    [Header("SE")]
    [SerializeField] private VisualInfo _AttackSE;
    [SerializeField] private VisualInfo _DamageSE;
    [SerializeField] private VisualInfo _BlowbackSE;
    [SerializeField] private VisualInfo _DeadSE;

    private UnitBase _targetUnit;
    private Transform _targetTransform;

    private float _shootTimer = 0;

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

        // 吹き飛ばし状態移行
        if (knockbackForce > 0)
        {
            blowback.SetVelocity(knockbackForce, pushdir);
            PlaySE(_BlowbackSE.SEName, _BlowbackSE.Volume);
            _stateMachine.ChangeState(Triggers.toBlowback);
        }

        // スタン状態移行
        if (damage > 0 && !_ignoreStan)
        {
            PlaySE(_DamageSE.SEName, _DamageSE.Volume);
            _stateMachine.ChangeState(Triggers.toStan);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();

        // 経験値アイテムドロップ
        if (_targetUnit != null)
        {
            float finalExp = UnitStatusData.baseExp;

            var expObj = Instantiate(_expItem, transform.position, Quaternion.identity);
            expObj.GetComponent<ExpItem>();
            expObj.Setup(_targetUnit);
            expObj.SetExp(finalExp);
        }

        PlaySE(_DeadSE.SEName, _DeadSE.Volume);
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
                if (SearchDistance(_ShootRange) && _shootTimer >= _ShootStateChangeTime)
                {
                    _stateMachine.ChangeState(Triggers.toShoot);
                }

                if (SearchDistance(_MoveStopRange))
                {
                    _stateMachine.ChangeState(Triggers.toStop);
                }
            }

            if (IsMatchingState(States.chaseStop))
            {
                if (SearchDistance(_ShootRange) && _shootTimer >= _ShootStateChangeTime)
                {
                    _stateMachine.ChangeState(Triggers.toShoot);
                }

                if (!SearchDistance(_MoveStopRange))
                {
                    _stateMachine.ChangeState(Triggers.toChase);
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
    /// 距離判定
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    private bool SearchDistance(float range)
    {
        var distance = Vector2.Distance(_targetTransform.position, Transform.position);
        if (distance <= range) return true;

        return false;
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

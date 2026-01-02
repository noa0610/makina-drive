using UnityEngine;
using System;

/// <summary>
/// 一定時間後に指定したステートへ遷移する前方射撃ステート
/// </summary>
public class ShootForward_LazyChange : ShootForward
{
    [SerializeField] private float _lazyChangeTime = 0.5f;
    protected bool _isBlock = false;
    public event Action OnCompleted;

    private string _lazyChange;
    private float _time = 0f;
    
    public ShootForward_LazyChange(BulletData bulletData, LayerMask targetLayer, string lazyChangeState, float lazyChangeTime) : base(bulletData, targetLayer)
    {
        _data = bulletData;
        _targetLayer = targetLayer;
        _lazyChange = lazyChangeState;
        _lazyChangeTime = lazyChangeTime;
    }
    public ShootForward_LazyChange(BulletData bulletData, LayerMask targetLayer) : base(bulletData, targetLayer)
    {
        _data = bulletData;
        _targetLayer = targetLayer;
    }
    public ShootForward_LazyChange(string lazyChangeState, float lazyChangeTime) : base()
    {
        _lazyChange = lazyChangeState;
        _lazyChangeTime = lazyChangeTime;
    }
    public ShootForward_LazyChange() : base(){}


    public override void Enter(IState previousIState, UnitBase parent)
    {
        base.Enter(previousIState, parent);
        _time = 0f;
        Action evt = null;
        evt = () =>
        {
            parent.stateMachine.LazyChange(_lazyChange);
            OnCompleted -= evt;
        };
        OnCompleted += evt;
    }

    public override void Stay(UnitBase parent, float deltaTime)
    {
        base.Stay(parent, deltaTime);
        _time += deltaTime;

        if (_time >= _lazyChangeTime)
        {
            OnCompleted?.Invoke();
            _time = 0f;
        }
    }

    public override bool AllowChange(IState nextState, UnitBase parent)
    {
        if (_isBlock) return false;
        return base.AllowChange(nextState, parent);
    }

    public void SetTime(float time)
    {
        _lazyChangeTime = time;
    }

    public void SetBlock(bool isBlock)
    {
        _isBlock = isBlock;
    }
}

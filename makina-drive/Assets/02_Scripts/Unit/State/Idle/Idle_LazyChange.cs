using UnityEngine;
using System;
using UnityEngine.Events;

/// <summary>
/// 一定時間後に指定したステートへ遷移する待機ステート
/// </summary>
public class Idle_LazyChange : Idle
{
    [SerializeField] private float _lazyChangeTime = 1.0f;
    protected bool _isBlock = false;
    public event Action OnCompleted;

    private string _lazyChange;
    private float _time = 0f;


    /// <param name="lazyChange">一定時間後に遷移するステート</param>
    /// <param name="lazyChangeTime">遷移の遅延</param>
    /// <param name="isBlock">遅延時間が終わるまで遷移を阻むかどうか</param>
    public Idle_LazyChange(string lazyChange, float lazyChangeTime, bool isBlock = false) : base()
    {
        _lazyChange = lazyChange;
        _lazyChangeTime = lazyChangeTime;
        _isBlock = isBlock;
    }

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

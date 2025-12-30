using UnityEngine;
using System;

public class StateComp : IState
{
    [SerializeField] private string _parentName;
    protected float _stateTime = 0f;
    protected int _waitFrame = 0;
    protected int _remainingWaitFrame = 0; // 残りの待機フレーム数
    // public event Action WaitTickHasCompleted;
    public StateComp(string parentName = "")
    {
        _parentName = parentName;
    }

    public virtual void Enter(IState previousState, UnitBase parent) // このステートに遷移したときの処理
    {
        _remainingWaitFrame = _waitFrame;
        _stateTime = 0f;
    }
    public virtual void Stay(UnitBase parent, float deltaTime)       // このステート中毎フレーム行う処理
    {
        _stateTime += deltaTime; // 毎フレームカウント
        Debug.Log($"{_stateTime}");

        if (_remainingWaitFrame > 0)
        {
            _remainingWaitFrame--;
        }
    }
    public virtual void Exit(IState nextState, UnitBase parent)      // このステートから離れる時行う処理
    {

    }
    
    public virtual bool AllowChange(IState nextState, UnitBase parent)
    {
        // _waitFrameが指定されている場合、待機フレーム完了まで遷移を許可しない
        if (_remainingWaitFrame > 0)
        {
            return false;
        }

        return true;
    }

    public virtual bool AllowEnter(IState previousState, UnitBase parent)
    {
        return true;
    }

    // 遅延遷移
    protected void ProcessLazyChange(UnitBase parent, string targetState, float delayTime)
    {
        if (string.IsNullOrEmpty(targetState)) return;

        if (_stateTime >= delayTime)
        {
            parent.stateMachine.LazyChange(targetState);
        }
    }

    public void SetWaitTick(int frame)
    {
        _waitFrame = frame;
        _remainingWaitFrame = frame;
    }
}

using UnityEngine;

public class StateComp : IState
{
    public virtual void Enter(IState previousState) // このステートに遷移したときの処理
    {
        
    }
    public virtual void Stay(float deltaTime)       // このステート中毎フレーム行う処理
    {
        
    }
    public virtual void Exit(IState nextState)      // このステートから離れる時行う処理
    {
        
    }
    public virtual void StateChange(IState nextState)
    {
        
    }
}

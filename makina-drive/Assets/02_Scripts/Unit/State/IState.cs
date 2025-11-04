using UnityEngine;

public interface IState
{
    void Enter(IState previousState); // このステートに遷移したときの処理
    void Stay(float deltaTime);       // このステート中毎フレーム行う処理
    void Exit(IState nextState);      // このステートから離れる時行う処理
    void StateChange(IState nextState);
}

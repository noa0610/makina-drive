using UnityEngine;

public interface IState
{
    void Enter(IState previousState, UnitBase parent); // このステートに遷移したときの処理
    void Stay(UnitBase parent, float deltaTime);       // このステート中毎フレーム行う処理
    void Exit(IState nextState, UnitBase parent);      // このステートから離れる時行う処理

    /// <summary>
    /// falseの場合IStateMachineは遷移をあきらめる
    /// </summary>
    bool AllowChange(IState nextState, UnitBase parent);

    bool AllowEnter(IState previousState, UnitBase parent);

}

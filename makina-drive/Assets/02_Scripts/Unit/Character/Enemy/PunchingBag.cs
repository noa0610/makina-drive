using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 攻撃テスト用エネミー
/// </summary>
public class PunchingBag : UnitBase
{
    #region === State ===
    private enum States
    {
        none,
        idle,
        knockback
    }

    private enum Triggers
    {
        none,
        damege,
        knockbackEnd
    }

    // ステート登録
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.damege, States.idle, ""),
        };
        var knockbackTrigger = new[]
        {
            (Triggers.knockbackEnd, States.knockback, ""),
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.knockback, knockbackTrigger);

        /* 待機 */
        var idle = new Idle();
        _stateMachine.AddState(States.idle, idle);

        /* ノックバック */
        var knockback = new Idle();
        _stateMachine.AddState(States.knockback, knockback);
    }
    #endregion




    [Header("固有設定")]
    private static readonly Dictionary<States, string> _stateNames = EnumWrapper.GetValueNameMap<States>();



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

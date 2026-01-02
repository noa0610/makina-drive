using System;
using System.Collections.Generic;

public interface IStateMachine
{
    Dictionary<string, StateInfo> StateMap { get; }

    StateInfo CurrentState { get; }

    /// <summary>
    /// もし現在のレイヤーで指定された遷移先が見つからなかった場合、
    /// デフォルトレイヤーから探すかどうか (デフォルトはtrue)
    /// </summary>
    bool UseDefaultLayerIfMissingTransition { get; set; }

    /// <summary>
    /// デフォルトの遷移表。（fromState, trigger）→ toState  
    /// ※モード依存の定義が無い場合のフォールバックとして利用される
    /// </summary>
    Dictionary<(string state, string trigger), (string state, string animetrigger)> TransitionGroup { get; }

    /// <summary>
    /// どの状態からでも遷移できるステートのグループ
    /// </summary>
    Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> AnyTransitionGroup { get; }

    // ======================
    // ステート操作
    // ======================
    bool ChangeState(string trigger);
    void LazyChange(string trigger);
    bool ChangeState<T>(T trigger) where T : Enum;
    void LazyChange<T>(T request) where T : Enum;
    void UpdateMachine(float deltaTime);

    // ======================
    // ステート登録
    // ======================

    /// <summary>ステートの追加</summary>
    void AddState(string key, StateComp state, params string[] tag);
    /// <summary>ステートの追加（Enumキー対応）</summary>
    void AddState<T>(T key, StateComp state, params string[] tags) where T : Enum;

    // ======================
    // 遷移定義
    // ======================
    void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "");
    void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
        where TState : Enum where TTrig : Enum;
    
    /// <summary>複数の遷移を一度に追加（fromState、toStateはEnum）</summary>
    IStateMachine AddTransition<TState, TTrig>(TState fromState, params (TTrig trigger, TState toState, string animationTrigger)[] transitions)
        where TState : Enum where TTrig : Enum;
    
    /// <summary>複数の遷移を一度に追加（fromStateは文字列、toStateはEnum）</summary>
    IStateMachine AddTransition<TState, TTrig>(string fromState, params (TTrig trigger, TState toState, string animationTrigger)[] transitions)
        where TState : Enum where TTrig : Enum;

    bool SetStateDirect(string target);
    void SetStateDirectLazy(string target);
    public StateInfo GetStateInfo(IState state);

    void Awake(string startStateKey = "idle", bool log = false);
}

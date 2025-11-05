using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IStateMachine
{
    Dictionary<string, IState> stateMap { get; }
    IState CurrentState { get; }
    /// <summary>
    /// fromState, trigger ⇒ toState
    /// </summary>
    Dictionary<(string state, string trigger), (string state, string animatrigger)> TransmissionGruop { get; }

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

    void AddState(string key, IState state);

    /// <summary> 
    /// fromState, trigger ⇒ toState  <br/>
    /// (あるステートでこの条件を満たすと ⇒ あのステートに遷移)
    /// </summary>
    void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "");
    void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
        where TState : Enum where TTrig : Enum;

    bool SetStateDirect(string target);
    void SetStateDirectLazy(string target);

    void Awake(string startStateKey = "idle");
}

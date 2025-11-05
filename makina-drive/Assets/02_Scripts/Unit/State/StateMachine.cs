using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class StateMachine /*: IStateMachine*/
{
//     private readonly Dictionary<string, IState> stateMap = new();
//     private IState _currentState;
//     public IState CurrentState => _currentState;

//     private readonly Dictionary<(string state, string trigger), (string state, string animatrigger)> _transmissionGruop = new();
//     /// <summary>
//     /// fromState, trigger ⇒ toState
//     /// </summary>
//     public Dictionary<(string state, string trigger), (string state, string animatrigger)> TransmissionGruop => _transmissionGruop;

//     public StateMachine(UnitBase parent) : this(parent, new NullAnimationDriver()) { }
//     public StateMachine(UnitBase parent)
//     {
//         _parent = parent;
//     }

//     // ======================
//     // ステート操作
//     // ======================
//     public bool ChangeState(string trigger)
//     {
//         return false;
//     }
//     public void LazyChange(string trigger)
//     {

//     }
//     public bool ChangeState<T>(T trigger) where T : Enum
//     {
//         return false;
//     }
//     public void LazyChange<T>(T request) where T : Enum
//     {

//     }
//     public void UpdateMachine(float deltaTime)
//     {

//     }

//     // ======================
//     // ステート登録
//     // ======================

//     public void AddState(string key, IState state)
//     {

//     }

//     public void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "");
//     {
//         _transmissionGruop[(fromState, trigger)] = (toState, animationTrigger);
//     }

// public void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
//     where TState : Enum where TTrig : Enum
//     => AddTransition(fromState.ToString(), trigger.ToString(), toState, animationTrigger);

// public bool SetStateDirect(string target)
// {
//     return false;
// }

// public void SetStateDirectLazy(string target)
// {

// }

// public void Awake(string startStateKey = "idle")
// {

// }
}

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public class StateMachine : IStateMachine
{
    private readonly UnitBase _parent;
    private readonly IAnimationDriver _anim;

    private readonly Dictionary<string, StateInfo> _stateMap = new();
    private readonly Dictionary<(string state, string trigger), (string state, string animatrigger)> _transitionGroup = new();
    private readonly Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> _anyTransitionGroup = new();

    public string CurrentLayer { get; private set; } = "Default";
    private StateInfo _currentState;
    private readonly Queue<string> _requests = new();
    private readonly Queue<string> _directRequests = new();

    public Dictionary<string, StateInfo> StateMap => _stateMap;
    public StateInfo CurrentState => _currentState;
    public Animator _animator;

    public Dictionary<(string state, string trigger), (string state, string animetrigger)> TransitionGroup => _transitionGroup;
    public Dictionary<(string layer, string trigger), (string toState, string animeTrigger)> AnyTransitionGroup => _anyTransitionGroup;
    public bool UseDefaultLayerIfMissingTransition { get; set; } = true;

    public StateMachine(UnitBase parent) : this(parent, new NullAnimationDriver()) { }
    public StateMachine(UnitBase parent, IAnimationDriver animationDriver)
    {
        _parent = parent;
        _anim = animationDriver ?? new NullAnimationDriver();
        _anim.CurrentLayer = CurrentLayer;
    }

    // ======================
    // ステート操作
    // ======================
    private bool Change((string state, string animetrigger) trs)
    {
        var to = _stateMap[trs.state];
        if (_currentState.Instance.AllowChange(to.Instance, _parent) && to.Instance.AllowEnter(_currentState.state, _parent))
        {
            var from = _currentState.state;
            var fromKey = _currentState.key;

            from.Exit(to.Instance, _parent);
            _currentState = to;

            _anim.OnTransition(fromKey, _currentState.key, trs.animetrigger);

            to.Instance.Enter(from, _parent);
            return true;
        }
        return false;
    }
    public bool ChangeState(string trigger)
    {
        if (_anyTransitionGroup.TryGetValue((CurrentLayer, trigger), out var trs))
        {
            return Change(trs);
        }

        // Default
        if (_transitionGroup.TryGetValue((_currentState.key, trigger), out trs))
        {
            return Change(trs);
        }
        return false;
    }
    public bool ChangeState<T>(T trigger) where T : System.Enum
            => ChangeState(trigger.ToString());

    public void LazyChange(string trigger)
    {
        if (!string.IsNullOrEmpty(trigger))
            _requests.Enqueue(trigger);
    }

    public void LazyChange<T>(T request) where T : System.Enum
            => LazyChange(request.ToString());

    public void UpdateMachine(float deltaTime)
    {
        while (_directRequests.Count > 0)
        {
            var direct = _directRequests.Dequeue();
            if (SetStateDirect(direct)) return;
        }
        while (_requests.Count > 0)
        {
            var trig = _requests.Dequeue();
            if (ChangeState(trig)) return;
        }
        _currentState.state.Stay(_parent, deltaTime);
    }

    // ======================
    // ステート登録
    // ======================
    public void AddState(string key, StateComp state, params string[] tags)
    {
        _stateMap[key] = new StateInfo(key, state, tags);
        if (state is IRigidbodyUser user)
        {
            if (_parent.TryGetComponent<Rigidbody2D>(out var rb)) user.SetRB2(rb);
            else Debug.LogWarning("RigitBody が未設定");
        }
    }

    public void AddState<T>(T key, StateComp state, params string[] tags) where T : System.Enum
        => AddState(key.ToString(), state, tags);

    public void AddTransition(string fromState, string trigger, string toState, string animationTrigger = "")
    {
        _transitionGroup[(fromState, trigger)] = (toState, animationTrigger);
    }

    public void AddTransition<TState, TTrig>(TState fromState, TTrig trigger, string toState, string animationTrigger = "")
        where TState : Enum where TTrig : Enum
        => AddTransition(fromState.ToString(), trigger.ToString(), toState, animationTrigger);


    public IStateMachine AddTransition<TState, TTrig>(TState fromState, params (TTrig trigger, TState toState, string animationTrigger)[] transitions)
        where TState : Enum where TTrig : Enum
    {
        foreach (var (trigger, toState, animationTrigger) in transitions)
        {
            AddTransition(fromState.ToString(), trigger.ToString(), toState.ToString(), animationTrigger);
        }
        return this;
    }
    public IStateMachine AddTransition<TState, TTrig>(string fromState, params (TTrig trigger, TState toState, string animationTrigger)[] transitions)
        where TState : Enum where TTrig : Enum
    {
        foreach (var (trigger, toState, animationTrigger) in transitions)
        {
            AddTransition(fromState, trigger.ToString(), toState.ToString(), animationTrigger);
        }
        return this;
    }

    public bool SetStateDirect(string target)
    {
        var tmp = _currentState.state;
        if (_stateMap.TryGetValue(target, out var state))
        {
            var prevKey = _currentState.key;
            _currentState = state;
            _anim.OnSetState(target); // ★アニメーション委譲（直接セット時）
            _currentState.state.Enter(tmp, _parent);
            return true;
        }
        return false;
    }

    public void SetStateDirectLazy(string target)
    {
        if (!string.IsNullOrEmpty(target))
            _directRequests.Enqueue(target);
    }

    public StateInfo GetStateInfo(IState state)
    {
        foreach(var info in _stateMap.Values)
        {
            if(info.Instance == state) return info;
        }
        return default;
    }

    public void Awake(string startStateKey = "idle", bool log = false)
    {
        if (_stateMap.ContainsKey(startStateKey))
        {
            SetStateDirect(startStateKey);
        }
        else
        {
            startStateKey = char.ToUpper(startStateKey[0]) + startStateKey.Substring(1);
            if (_stateMap.ContainsKey(startStateKey))
                SetStateDirect(startStateKey);
            else
                throw new System.ArgumentException($"Unknown start state: {startStateKey}");
        }
        if (!log) return;
        // ログ
        Debug.Log($"[StateMachine] Start State: {_currentState.key}");
        foreach (var t in _transitionGroup)
        {
            Debug.Log($"[StateMachine] Transition added: {t.Key.state} --({t.Key.trigger})-> {t.Value.state}");
        }
        foreach (var t in _anyTransitionGroup)
        {
            Debug.Log($"[StateMachine] Any Transition added: [Layer:{t.Key.layer}] --({t.Key.trigger})-> {t.Value.toState}");
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

/// <summary>
/// エフェクトのインスタンスにアタッチするクラス
/// </summary>
public class EffectInstance : MonoBehaviour
{
    [SerializeField] private EffectDataBase _data;
    public EffectDataBase EffectData => _data;

    private Transform _target;
    private float _timer;
    private float _delayTimer;
    private bool _isStarted;
    private bool _isStopping = false;
    private List<ParticleSystem> _particles = new List<ParticleSystem>();
    private List<Renderer> _renderers = new List<Renderer>();

    public Action<EffectInstance> OnEffectComplete;

    private void Awake()
    {
        GetComponentsInChildren(true, _particles);
        GetComponentsInChildren(true, _renderers);
    }
    
    // エフェクトの再生開始
    public void Play(Transform target = null)
    {
        _target = target;
        _timer = 0;
        _delayTimer = 0;
        _isStopping = false;

        // データから初期回転を反映
        transform.localRotation = Quaternion.Euler(_data.initialEulerAngles);

        if(_data.startDelay > 0)
        {
            _isStarted = false;
            ToggleVisuals(false);
        }
        else
        {
            StartVisuals();
        }
    }

    public void Init(EffectDataBase data, Transform target = null)
    {
        _data = data;
        _target = target;
        _timer = 0;
        _delayTimer = 0;
        _isStarted = false;
        _isStopping = false;

        

        if (_data.startDelay > 0)
        {
            GetComponentsInChildren(true, _particles);
            GetComponentsInChildren(true, _renderers);
            ToggleVisuals(false);
        }
        else
        {
            _isStarted = true;
        }
    }

    private void FixedUpdate()
    {
        if (_isStopping || _data == null) return;

        // ディレイ処理
        if (!_isStarted)
        {
            _delayTimer += Time.fixedDeltaTime;
            if (_delayTimer >= _data.startDelay)
            {
                StartVisuals();
            }
            return;
        }
        
        UpdatePosition();

        // 寿命処理
        if(!_data.loopForever)
        {
            _timer += Time.fixedDeltaTime;
            if(_timer >= _data.duration)
            {
                Stop();
            }
        }

        if (_target == null) return;
    }

    private void StartVisuals()
    {
        _isStarted = true;
        ToggleVisuals(true);
    }

    private void UpdatePosition()
    {
        switch (_data.attachType)
        {
            case EffectAttachType.FollowTarget:
                transform.position = _target.position + GetOffsetWithFlip();
                break;
            case EffectAttachType.BiniToBone:
                transform.position = _target.TransformPoint(_data.offset);
                if (!_data.ignoreFlip) transform.rotation = _target.rotation;
                transform.localScale = _target.localScale;
                break;
        }
    }

    // プレイヤーの向きに合わせてオフセットを計算
    private Vector3 GetOffsetWithFlip()
    {
        Vector3 offset = _data.offset;
        // ターゲットのScale.xが負（左向き）ならオフセットのXを反転
        if (_target.lossyScale.x < 0)
        {
            offset.x *= -1;
        }
        return offset;
    }

    public void ToggleVisuals(bool show)
    {
        foreach (var r in _renderers) if (r != null) r.enabled = show;
        foreach (var p in _particles)
        {
            if (p == null) continue;
            if (show) p.Play(true);
            else p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // 再生の停止
    public void Stop()
    {
        if (_isStopping) return;
        _isStopping = true;

        if (_data.stopType == EffectStopType.StopEmitting)
        {
            foreach (var p in _particles) if(p != null) p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Invoke(nameof(NotifyCOmplete), 2.0f);
        }
        else
        {
            NotifyCOmplete();
        }
    }

    // 再生完全終了
    public void NotifyCOmplete()
    {
        OnEffectComplete?.Invoke(this);
    }
}

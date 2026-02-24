using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

/// <summary>
/// エフェクトのインスタンスにアタッチするクラス
/// </summary>
public class EffectInstance : MonoBehaviour
{
    [SerializeField] private EffectDataBase _data;
    public EffectDataBase EffectData => _data;

    private Transform _target;          // 位置参照対象
    private Transform _directionTarget; // 向き参照対象（指定がない場合、targetの向きを参照）
    private Vector3 _baseScale = Vector3.one;
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

        // エフェクトスケーリングモード変更（Hierarchyにし、サイズ変更の影響をまとめて受ける）
        foreach (var p in _particles)
        {
            if (p != null)
            {
                var main = p.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }
    }

    // エフェクトの再生開始
    public void Play(Transform target = null, Transform directionTarget = null)
    {
        _target = target;
        _directionTarget = directionTarget ?? target;
        _timer = 0;
        _delayTimer = 0;
        _isStopping = false;

        UpdateTransform();

        if (_data.startDelay > 0)
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

    public void SetScale(Vector3 size)
    {
        _baseScale = size;
        transform.localScale = _baseScale;
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

        UpdateTransform();

        // 寿命処理
        if (!_data.loopForever)
        {
            _timer += Time.fixedDeltaTime;
            if (_timer >= _data.duration)
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

    private void UpdateTransform()
    {
        Vector3 finalOffset = _data.offset;
        bool isFlipped = IsFlipped();
        if (isFlipped && !_data.ignoreFlip)
        {
            finalOffset.x *= -1;
        }

        // --- 位置の更新 ---
        switch (_data.attachType)
        {
            case EffectAttachType.FixedPosition:
                break;

            case EffectAttachType.FollowTarget:
                if (_target == null) return;
                transform.position = _target.position + finalOffset;
                break;

            case EffectAttachType.BindToBone:
                if (_target == null) return;
                transform.position = _target.TransformPoint(_data.offset);
                break;
        }

        // --- 回転の更新 ---
        Quaternion baseRotation = GetModifiedRotation(isFlipped);

        if (_data.attachType == EffectAttachType.BindToBone)
        {
            // ボーンの回転 * エフェクト自体の設定角度
            transform.rotation = _target.rotation * baseRotation;
        }
        else
        {
            // 固定または追従時は、ワールド回転として適用
            transform.rotation = baseRotation;
        }

        // --- スケールの更新 ---
        // (反転をScaleで行う場合)
        if (_data.attachType == EffectAttachType.BindToBone)
        {
            // ボーンのスケール × 基準サイズ
            if (_target != null)
            {
                transform.localScale = Vector3.Scale(_target.localScale, _baseScale);
            }
        }
        else if (!_data.ignoreFlip)
        {
            // 反転フラグ × 基準サイズ
            Vector3 scale = _baseScale;
            if (IsFlipped()) scale.x *= -1;
            transform.localScale = scale;
        }
        else
        {
            // 反転無視の場合は基準サイズのみ適用
            transform.localScale = _baseScale;
        }
    }

    private Quaternion GetModifiedRotation(bool isFlipped)
    {
        Vector3 angles = _data.initialEulerAngles;

        if (isFlipped && _data.mirrorRotation && !_data.ignoreFlip)
        {
            // Z軸回転の鏡写し（2Dゲームで一般的な、進行方向に対する反転）
            // 例：30度で右上に飛ぶエフェクトなら、反転時は150度で左上に飛ぶようにする
            angles.z = 180f - angles.z;

            // Y 軸なども反転させる場合はここに追加する
            // angles.y += 180f; 
        }

        return Quaternion.Euler(angles);
    }

    // 向き参照対象に基づいた向き判定
    private bool IsFlipped()
    {
        if (_directionTarget == null) return false;

        return _directionTarget.lossyScale.x < 0;
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
            foreach (var p in _particles) if (p != null) p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Invoke(nameof(NotifyComplete), 2.0f);
        }
        else
        {
            NotifyComplete();
        }
    }

    // 再生完全終了
    public void NotifyComplete()
    {
        OnEffectComplete?.Invoke(this);
    }
}

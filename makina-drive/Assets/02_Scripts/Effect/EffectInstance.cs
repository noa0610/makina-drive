using System.Collections.Generic;
using UnityEngine;

public class EffectInstance : MonoBehaviour
{
    private EffectDataBase _data;
    private Transform _target;
    private float _timer;
    private float _delayTimer;
    private bool _isStarted;
    private List<ParticleSystem> _particles = new List<ParticleSystem>();
    private List<Renderer> _renderers = new List<Renderer>();

    public void Init(EffectDataBase data, Transform target = null)
    {
        _data = data;
        _target = target;
        _timer = 0;
        _delayTimer = 0;
        _isStarted = false;

        transform.localRotation = Quaternion.Euler(_data.initialEulerAngles);

        if(_data.startDelay > 0)
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

    private void LateUpdate()
    {
        if (_data == null) return;

        if(!_isStarted)
        {
            _delayTimer += Time.deltaTime;

            if (_delayTimer >= _data.startDelay)
            {
                _isStarted = true;
                ToggleVisuals(true);
            }
            else
            {
                // ディレイ中も追従が必要な場合は位置更新のみ行う
                if (_target != null) UpdatePosition();
                return;
            }
        }

        _timer += Time.deltaTime;
        if (_timer >= _data.duration)
        {
            // 本来はここでプールに戻すが、今回はシンプルにDestroy
            Destroy(gameObject);
            return;
        }

        if (_target == null) return;
        UpdatePosition();
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
                if(!_data.ignoreFlip) transform.rotation = _target.rotation;
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
        foreach(var r in _renderers) if(r != null) r.enabled = show;
        foreach(var p in _particles)
        {
            if(p == null) continue;
            if(show) p.Play(true);
            else p.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // private void StopEffect()
    // {
    //     if(_data.stopType == EffectStopType.StopEmitting)
    //     {
    //         foreach(var p in _particles) p.Stop();

    //         Invoke(nameof(ReturnToPool), 2.0f);
    //     }
    //     else
    //     {
    //         ReturnToPool();
    //     }
    // }

    private void ReturnToPool() => EffectManager.instance.ReturnToPool(gameObject, _data.effectName);
    private void SetVisible(bool visible) => gameObject.SetActive(visible);
}

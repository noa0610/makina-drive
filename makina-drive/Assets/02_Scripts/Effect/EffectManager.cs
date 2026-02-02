using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonBehavior<EffectManager>
{
    [SerializeField] private List<EffectInstance> _effectPrefabs;
    private Dictionary<string, EffectInstance> _prefabMap = new();
    private Dictionary<string, Stack<EffectInstance>> _poolMap = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var prefab in _effectPrefabs)
        {
            if (prefab != null)
            {
                _prefabMap[prefab.EffectData.effectName] = prefab;
            }
        }
    }

    // エフェクトを再生
    public EffectInstance Play(string effectName, Vector3 position, Transform target = null)
    {
        if (!_prefabMap.TryGetValue(effectName, out var prefab))
        {
            Debug.LogWarning($"Effect: {effectName} が見つかりません。");
            return null;
        }

        // プールから取得
        EffectInstance instance = GetFromPool(prefab);

        // 生成位置設定
        Vector3 spawnPos = position;
        if (instance.EffectData.useRandomOffset)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * instance.EffectData.randomRange;
            spawnPos += new Vector3(randomPoint.x, randomPoint.y, 0);
        }

        instance.transform.position = spawnPos;

        // イベント購読
        instance.OnEffectComplete -= HandleEffectComplete;
        instance.OnEffectComplete += HandleEffectComplete;

        instance.Play(target);
        
        return instance;
    }

    private EffectInstance GetFromPool(EffectInstance prefab)
    {
        string key = prefab.EffectData.effectName;

        if(!_poolMap.ContainsKey(key))
        {
            _poolMap[key] = new Stack<EffectInstance>();
        }

        if(_poolMap[key].Count > 0)
        {
            EffectInstance pooledInstance = _poolMap[key].Pop();
            pooledInstance.gameObject.SetActive(true);
            return pooledInstance;
        }

        // 新規作成
        return Instantiate(prefab, transform);
    }

    private void HandleEffectComplete(EffectInstance instance)
    {
        // 購読解除
        instance.OnEffectComplete -= HandleEffectComplete;

        // プールに戻す
        ReturnToPool(instance);
    }

    public void ReturnToPool(EffectInstance instance)
    {
        string key = instance.EffectData.effectName;

        instance.gameObject.SetActive(false);
        
        if(!_poolMap.ContainsKey(key))
        {
            _poolMap[key] = new Stack<EffectInstance>();
        }

        _poolMap[key].Push(instance);
    }
}

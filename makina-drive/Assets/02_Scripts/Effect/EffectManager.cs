using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonBehavior<EffectManager>
{
    [SerializeField] private List<EffectDataBase> _effectList;
    private Dictionary<string, EffectDataBase> _dataMap = new();
    private Dictionary<string, Stack<GameObject>> _poolMap = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var d in _effectList) _dataMap[d.effectName] = d;
    }

    public void PlayEffect(EffectDataBase data, Transform target)
    {
        if (data == null || data.effectPrefab == null) return;

        GameObject go = Instantiate(data.effectPrefab);
        EffectInstance instance = go.AddComponent<EffectInstance>();
        instance.Init(data, target);
    }

    public void Play(string effectName, Vector3 position, Transform target = null)
    {
        if (!_dataMap.TryGetValue(effectName, out var data)) return;

        Vector3 spawnPos = position;
        if (data.useRandomOffset)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * data.randomRange;
            spawnPos += new Vector3(randomPoint.x, randomPoint.y, 0);
        }

        GameObject obj = GetFromPool(data);
        obj.transform.position = spawnPos;

        if (!obj.TryGetComponent<EffectInstance>(out var instance))
        {
            instance = obj.AddComponent<EffectInstance>();
        }
        instance.Init(data, target);
    }

    private GameObject GetFromPool(EffectDataBase data)
    {
        if (!_poolMap.ContainsKey(data.effectName)) _poolMap[data.effectName] = new Stack<GameObject>();

        if (_poolMap[data.effectName].Count > 0)
        {
            GameObject obj = _poolMap[data.effectName].Pop();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(data.effectPrefab);
    }

    public void ReturnToPool(GameObject obj, string name)
    {
        obj.SetActive(false);
        _poolMap[name].Push(obj);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : SingletonBehavior<EffectManager>
{
    [SerializeField] private EffectDataBase _db;
    private Dictionary<String, Stack<GameObject>> _pool = new();

    public void Play(string effectName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = _db.GetPrefab(effectName);
        if (prefab == null) return;

        // GameObject effect = GetFromPool(effectName, prefab);
        // effect.transform.SetPositionAndRotation(position, rotation);
        // effect.SetActive(true);

        // StartCoroutine(ReturnToPoolRoutine(effectName, effectName, 2.0f));
    }
    
    // private GameObject GetFromPool(string name, GameObject prefab){ }
}

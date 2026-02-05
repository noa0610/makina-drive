using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 楕円形の形状になるようにユニットを生成
/// </summary>
[Serializable]
public class SpawnOval : ISpawnComponent
{
    public GameObject target { get; set; }
    [SerializeField] private float _radiusX = 10f; // 横の半径
    [SerializeField] private float _radiusY = 7f;  // 盾の半径
    
    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            float angle = (360f / pool.Count) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * _radiusX, Mathf.Sin(angle) * _radiusY, 0);
            pool[i].transform.position = target.transform.position + offset;
        }
        return pool;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 長方形の形状になるようにユニットを生成
/// </summary>
[Serializable]
public class SpawnRectangle : ISpawnComponent
{
    public GameObject target { get; set; }
    [SerializeField] private Vector2 _size = new Vector2(20, 15);
    
    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        foreach (var unit in pool)
        {
            Vector3 pos = Vector3.zero;
            float side = UnityEngine.Random.value;if (side < 0.25f) pos = new Vector3(-_size.x / 2, UnityEngine.Random.Range(-_size.y / 2, _size.y / 2)); // 左
            else if (side < 0.5f) pos = new Vector3(_size.x / 2, UnityEngine.Random.Range(-_size.y / 2, _size.y / 2));  // 右
            else if (side < 0.75f) pos = new Vector3(UnityEngine.Random.Range(-_size.x / 2, _size.x / 2), _size.y / 2); // 上側
            else pos = new Vector3(UnityEngine.Random.Range(-_size.x / 2, _size.x / 2), -_size.y / 2);                  // 下側

            unit.transform.position = target.transform.position + pos;
        }
        return pool;
    }
}

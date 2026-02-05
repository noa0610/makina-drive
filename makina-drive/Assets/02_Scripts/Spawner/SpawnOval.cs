using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 楕円形の形状になるように均等にユニットを生成
/// </summary>
[Serializable]
public class SpawnOval : ISpawnComponent
{
    public GameObject target { get; set; }
    [SerializeField] private float _radiusX = 10f; // 横の半径
    [SerializeField] private float _radiusY = 7f;  // 盾の半径
    [SerializeField, Range(0, 360)] private float _startAngle = 0f; // 開始地点をずらす
    [SerializeField] private float _jitter = 0.5f; // 均等の中にも少しランダム性を出す
    
    public List<UnitBase> Execute(List<UnitBase> pool)
    {   
        float angleStep = 360f / pool.Count;

        for (int i = 0; i < pool.Count; i++)
        {
            // 各キャラの角度を計算
            float angle = (_startAngle + (angleStep * i)) * Mathf.Deg2Rad;

            float currentJitterX = UnityEngine.Random.Range(-_jitter, _jitter);
            float currentJitterY = UnityEngine.Random.Range(-_jitter, _jitter);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * _radiusX + currentJitterX, 
                Mathf.Sin(angle) * _radiusY + currentJitterY,
                0
            );
            pool[i].transform.position = target.transform.position + offset;
        }
        return pool;
    }
}

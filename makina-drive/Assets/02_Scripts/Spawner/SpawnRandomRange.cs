using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターゲットの周囲の指定した最小・最大距離の範囲内にユニットを生成（ドーナツ状）
/// </summary>
[Serializable]
public class SpawnRandomRange : ISpawnComponent
{
    public GameObject target { get; set; }
    [SerializeField] private float _maxDistance = 3;
    [SerializeField] private float _minDistance = 9;

    public List<Vector3> GetPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        if (count <= 0) return positions;

        Vector3 center = target != null ? target.transform.position : Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            // ランダムな方向を決定
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            // 最小～最大の範囲でランダムな距離を決定
            float distance = UnityEngine.Random.Range(_minDistance, _maxDistance);

            positions.Add(center + (direction * distance));
        }
        return positions;
    }

    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            // ランダムな方向を決定
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            // 最小～最大の範囲でランダムな距離を決定
            float distance = UnityEngine.Random.Range(_minDistance, _maxDistance);

            pool[i].transform.position = target.transform.position + (direction * distance);
        }
        return pool;
    }

    public void ApplyParameters(string paramString)
    {
        if (string.IsNullOrEmpty(paramString)) return;

        // 「minDistance:3;maxDistance:9」のような形式を想定
        string[] pairs = paramString.Split(';');
        foreach(string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if(kv.Length < 2) continue;

            string key = kv[0].Trim().ToLower();
            string value = kv[1].Trim();

            switch(key)
            {
                case "minDistance": float.TryParse(value, out _minDistance); break;
                case "maxDistance": float.TryParse(value, out _maxDistance); break;
            }
        }
    }
}

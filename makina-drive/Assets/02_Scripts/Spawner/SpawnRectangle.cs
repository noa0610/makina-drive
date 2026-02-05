using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 長方形の形状になるように均等にユニットを生成
/// </summary>
[Serializable]
public class SpawnRectangle : ISpawnComponent
{
    public GameObject target { get; set; }
    [SerializeField] private Vector2 _size = new Vector2(20, 15);
    
    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        float width = _size.x;
        float height = _size.y;
        float perimeter = (width + height) * 2f; // 外周の合計

        float step = perimeter / pool.Count; // 一体辺りの間隔

        for (int i = 0; i < pool.Count; i++)
        {
            float distance = step * i;
            Vector3 pos = CalculatePointOnRect(distance, width, height);
            pool[i].transform.position = target.transform.position + pos;
        }
        return pool;
    }

    private Vector3 CalculatePointOnRect(float d, float w, float h)
    {
        float halfW = w / 2f;
        float halfH = h / 2f;

        // 右上を起点に時計回りに配置していくロジック

        if(d < w) // 上辺
            return new Vector3(-halfW + d, halfH, 0);
        d -= w;
        if(d < h) // 右辺
            return new Vector3(halfW, halfH - d, 0);
        d -= h;
        if(d < w) // 下辺
            return new Vector3(halfW - d, -halfH, 0);
        d -= w;
                  // 右辺
        return new Vector3(-halfW, -halfH + d, 0);
    }
}

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
    [SerializeField] private Vector2 _size = new Vector2(10, 7);

    
    public List<Vector3> GetPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        if (count <= 0) return positions;

        float width = _size.x;
        float height = _size.y;
        float perimeter = (width + height) * 2f; // 外周の合計

        float step = perimeter / count; // 一体辺りの間隔

        for (int i = 0; i < count; i++)
        {
            float distance = step * i;
            Vector3 pos = CalculatePointOnRect(distance, width, height);
            positions.Add(target.transform.position + pos);
        }
        return positions;
    }
    
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

    public void ApplyParameters(string paramString)
    {
        if (string.IsNullOrEmpty(paramString)) return;

        // 「sizex:10;sizey:7」のような形式を想定
        string[] pairs = paramString.Split(';');
        foreach(string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if(kv.Length < 2) continue;

            string key = kv[0].Trim().ToLower();
            string value = kv[1].Trim();

            switch(key)
            {
                case "sizex": float.TryParse(value, out _size.x); break;
                case "sizey": float.TryParse(value, out _size.y); break;
            }
        }
    }
}

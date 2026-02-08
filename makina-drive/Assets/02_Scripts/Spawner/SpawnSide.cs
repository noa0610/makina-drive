using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指定した方向からユニットを生成
/// </summary>
[Serializable]
public class SpawnSide : ISpawnComponent
{
    private enum SpawnDirection
    {
        Right,
        Left,
        Up,
        Down,
        UpperRight,
        UpperLeft,
        LowerRight,
        LowerLeft
    }
    [SerializeField] private SpawnDirection spawnDirection;
    public GameObject target { get; set; }

    public List<Vector3> GetPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();
        if (count <= 0) return positions;

        Camera cam = Camera.main;
        for (int i = 0; i < count; i++)
        {
            Vector3 viewportPos = new Vector3(0.5f, 0.5f, 10);

            switch (spawnDirection)
            {
                case SpawnDirection.Right: viewportPos.x = 1.1f; break;
                case SpawnDirection.Left: viewportPos.x = -0.1f; break;
                case SpawnDirection.Up: viewportPos.y = 1.1f; break;
                case SpawnDirection.Down: viewportPos.y = -0.1f; break;
                case SpawnDirection.UpperRight: viewportPos = new Vector3(1.1f, 1.1f, 10); break;
                case SpawnDirection.UpperLeft: viewportPos = new Vector3(-0.1f, 1.1f, 10); break;
                case SpawnDirection.LowerRight: viewportPos = new Vector3(1.1f, -0.1f, 10); break;
                case SpawnDirection.LowerLeft: viewportPos = new Vector3(-0.1f, -0.1f, 10); break;
            }

            // 上下左右のみの場合は、もう一方の軸をランダムにする
            if (spawnDirection == SpawnDirection.Right || spawnDirection == SpawnDirection.Left)
                viewportPos.y = UnityEngine.Random.value;
            else if (spawnDirection == SpawnDirection.Up || spawnDirection == SpawnDirection.Down)
                viewportPos.x = UnityEngine.Random.value;

            Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0;
            positions.Add(worldPos);
        }
        return positions;
    }

    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        Camera cam = Camera.main;
        foreach (var unit in pool)
        {
            Vector3 viewportPos = new Vector3(0.5f, 0.5f, 10);

            switch (spawnDirection)
            {
                case SpawnDirection.Right: viewportPos.x = 1.1f; break;
                case SpawnDirection.Left: viewportPos.x = -0.1f; break;
                case SpawnDirection.Up: viewportPos.y = 1.1f; break;
                case SpawnDirection.Down: viewportPos.y = -0.1f; break;
                case SpawnDirection.UpperRight: viewportPos = new Vector3(1.1f, 1.1f, 10); break;
                case SpawnDirection.UpperLeft: viewportPos = new Vector3(-0.1f, 1.1f, 10); break;
                case SpawnDirection.LowerRight: viewportPos = new Vector3(1.1f, -0.1f, 10); break;
                case SpawnDirection.LowerLeft: viewportPos = new Vector3(-0.1f, -0.1f, 10); break;
            }

            // 上下左右のみの場合は、もう一方の軸をランダムにする
            if (spawnDirection == SpawnDirection.Right || spawnDirection == SpawnDirection.Left)
                viewportPos.y = UnityEngine.Random.value;
            else if (spawnDirection == SpawnDirection.Up || spawnDirection == SpawnDirection.Down)
                viewportPos.x = UnityEngine.Random.value;

            Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0;
            unit.transform.position = worldPos;
        }
        return pool;
    }

    public void ApplyParameters(string paramString)
    {
        if (string.IsNullOrEmpty(paramString)) return;

        // 「spawnDirection:Right」や「spawnDirection:LowerLeft」のような形式を想定
        string[] pairs = paramString.Split(';');
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if (kv.Length < 2) continue;

            string key = kv[0].Trim().ToLower();
            string value = kv[1].Trim();

            switch (key)
            {
                case "spawnDirection":
                    if(Enum.TryParse(value, true, out SpawnDirection result))
                    {
                        this.spawnDirection = result;
                    }
                    else
                    {
                        Debug.LogWarning($"SpawnSide: '{value}' は有効な方向ではありません。");
                    }
                    break;
            }
        }
    }
}

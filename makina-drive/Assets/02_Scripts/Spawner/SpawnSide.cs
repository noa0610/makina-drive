using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ランダムな方向からユニットを生成
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
}

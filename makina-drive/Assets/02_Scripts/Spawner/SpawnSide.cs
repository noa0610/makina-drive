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
    public enum SpawnDirection
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
    public SpawnDirection spawnDirection;
    public GameObject target { get; set; }

    public List<UnitBase> Execute(List<UnitBase> pool)
    {
        Camera cam = Camera.main;
        foreach (var unit in pool)
        {
            Vector3 viewportPos = new Vector3(0.5f, 0.5f, 10);
            
            switch (spawnDirection)
            {
                case SpawnDirection.Right: viewportPos.x = 1.2f; break;
                case SpawnDirection.Left:  viewportPos.x = -0.2f; break;
                case SpawnDirection.Up:    viewportPos.y = 1.2f; break;
                case SpawnDirection.Down:  viewportPos.y = -0.2f; break;
            }
            
            // 指定方向の反対側の軸はランダムに散らす
            if (spawnDirection == SpawnDirection.Right || spawnDirection == SpawnDirection.Left)
                viewportPos.y = UnityEngine.Random.value;
            else
                viewportPos.x = UnityEngine.Random.value;

            Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0;
            unit.transform.position = worldPos;
        }
        return pool;
    }
}

using UnityEngine;

public static class SpawnUtils
{
    // カメラの表示範囲から少し外側の座標を取得する
    public static Vector3 GetRandomOffScreenPosition(GameObject target, float margin = 2.0f)
    {
        Camera cam = Camera.main;
        // 0～1の範囲が画面内。-0.2や1.2にすることで画面外にする
        float side = UnityEngine.Random.value;
        Vector3 viewportPos = Vector3.zero;

        if (side < 0.25f) // 左
            viewportPos = new Vector3(-0.1f, UnityEngine.Random.value, 10);
        else if (side < 0.5f) // 右
            viewportPos = new Vector3(1.1f, UnityEngine.Random.value, 10);
        else if (side < 0.75f) // 上
            viewportPos = new Vector3(UnityEngine.Random.value, 1.1f, 10);
        else // 下
            viewportPos = new Vector3(UnityEngine.Random.value, -0.1f, 10);

        Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
        worldPos.z = 0; // 2DなのでZは0
        return worldPos;
    }
}
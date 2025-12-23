using UnityEngine;

/// <summary>
/// 入力方向にGizmoをゆっくりと曲げるテスト用クラス
/// </summary>
public class RotateVectorTest : MonoBehaviour
{
    [Header("現在の向き（徐々に変わる）")]
    public Vector2 direction = Vector2.right;

    [Header("Input の方向（キー入力）")]
    public Vector2 inputDirection;

    [Header("回転速度（度/秒）")]
    public float rotateSpeed = 360f;

    void Update()
    {
        // --- 入力取得（WASD / 矢印キー） ---
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector2(x, y).normalized;

        if (inputDirection.sqrMagnitude > 0.0f)
        {
            // --- 方向を徐々に回す ---
            direction = RotateTowards(direction, inputDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime);
        }
    }

    /// <summary>
    /// from を to に向けて maxRadiansDelta だけ回転させる
    /// </summary>
    Vector2 RotateTowards(Vector2 from, Vector2 to, float maxRadiansDelta)
    {
        float angle = Vector2.SignedAngle(from, to);
        float angleRad = angle * Mathf.Deg2Rad;

        // 角度が小さければ to へスナップ
        if (Mathf.Abs(angleRad) <= maxRadiansDelta)
        {
            return to.normalized;
        }

        // 回転方向に maxRadiansDelta 分だけ回す
        float newAngleRad = Mathf.Clamp(angleRad, -maxRadiansDelta, maxRadiansDelta);
        float newAngleDeg = newAngleRad * Mathf.Rad2Deg;

        return Quaternion.Euler(0, 0, newAngleDeg) * from;
    }

    // --- シーンビューに方向を描画 ---
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)inputDirection);
    }
}

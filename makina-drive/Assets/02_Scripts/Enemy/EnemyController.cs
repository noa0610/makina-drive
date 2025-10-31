using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Transform player; // ← 座標ではなく参照で持つ

    [Header("Move")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private bool use2D = true;
    [SerializeField] private bool useRigidbodyMove = true;
    [SerializeField] private bool rotateToMoveDir = true;

    private Rigidbody2D rb2d;
    private Rigidbody rb3d;

    public void SetTarget(Transform target) => player = target;  // ← 注入口

    private void Awake()
    {
        if (use2D) rb2d = GetComponent<Rigidbody2D>();
        else       rb3d = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!player) return;
        if (use2D) Move2D();
    }

    private void Move2D()
    {
        Vector2 pos = transform.position;
        Vector2 target = player.position;
        Vector2 to = target - pos;
        float dist = to.magnitude;
        if (dist <= stopDistance) { if (rb2d) rb2d.linearVelocity = Vector2.zero; return; }

        Vector2 dir = to / dist;
        if (rotateToMoveDir)
        {
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, ang);
        }

        Vector2 next = pos + dir * moveSpeed * Time.fixedDeltaTime;
        if (useRigidbodyMove && rb2d) rb2d.MovePosition(next);
        else                          transform.position = next;
    }
}

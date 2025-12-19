using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform player;         // ← PlayerのTransformをアサイン
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float interval = 2f;      // 何秒おきにスポーン処理を走らせるか
    [SerializeField] private int spawnPerTick = 2;     // 1回の処理で何体出すか
    [SerializeField] private int maxAlive = 50;        // 同時出現の上限

    [Header("Area (Ring around player)")]
    [SerializeField] private float radiusMin = 6f;     // プレイヤーから最小距離
    [SerializeField] private float radiusMax = 10f;    // プレイヤーから最大距離

    [Header("Collision Check")]
    [SerializeField] private bool use2D = true;        // 2Dならtrue, 3Dならfalse
    [SerializeField] private LayerMask blockMask;      // 壁/地形など障害物のレイヤー
    [SerializeField] private float clearRadius = 0.5f; // 出現位置の空き半径
    [SerializeField] private int searchAttempts = 10;  // 有効位置を探す試行回数

    private float timer;
    private int alive;

    private void Update()
    {
        if (player == null || enemyPrefab == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = interval;
            SpawnTick();
        }
    }

    private void SpawnTick()
    {
        for (int i = 0; i < spawnPerTick; i++)
        {
            if (alive >= maxAlive) break;

            if (TryGetSpawnPos(player.position, out var pos))
            {
                // 生成してすぐに敵へターゲット(Transform)を渡す
                var go = Instantiate(enemyPrefab, pos, Quaternion.identity);
                if (go.TryGetComponent<EnemyController>(out var ec))
                {
                    ec.SetTarget(player); // ← Transformを注入
                }

                // もし敵に死亡通知があるなら登録（下のEnemyLife例を参照）
                if (go.TryGetComponent<EnemyLife>(out var life))
                {
                    life.onDied += HandleEnemyDied;
                }
            }
        }
    }

    private void HandleEnemyDied(EnemyLife e)
    {
        alive = Mathf.Max(0, alive - 1);
    }

    private bool TryGetSpawnPos(Vector3 center, out Vector3 pos)
    {
        for (int i = 0; i < searchAttempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist  = Random.Range(radiusMin, radiusMax);

            Vector3 offset;
            if (use2D)
            {
                // X-Y平面（2D）
                offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * dist;
            }
            else
            {
                // X-Z平面（3D）
                offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * dist;
            }

            Vector3 candidate = center + offset;

            // その地点が障害物に埋まっていないか
            bool blocked = use2D
                ? Physics2D.OverlapCircle(candidate, clearRadius, blockMask) != null
                : Physics.CheckSphere(candidate, clearRadius, blockMask);

            if (blocked) continue;

            // プレイヤー→候補点の直線が壁で遮られていないか（視線チェック）
            bool sightBlocked = false;
            if (use2D)
            {
                var hit = Physics2D.Linecast(center, candidate, blockMask);
                sightBlocked = hit;
            }
            else
            {
                // 少し持ち上げてレイ（地形の段差対策）
                Vector3 from = center + Vector3.up * 0.5f;
                Vector3 to   = candidate + Vector3.up * 0.5f;
                sightBlocked = Physics.Linecast(from, to, blockMask);
            }
            if (sightBlocked) continue;

            pos = candidate;
            return true;
        }

        pos = default;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, radiusMin);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, radiusMax);
    }
}

// （任意）敵の死亡をGameManagerに伝えるための超簡易ライフ例
public class EnemyLife : MonoBehaviour
{
    public System.Action<EnemyLife> onDied;
    [SerializeField] private int hp = 10;

    public void Damage(int d)
    {
        hp -= d;
        if (hp <= 0)
        {
            onDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}

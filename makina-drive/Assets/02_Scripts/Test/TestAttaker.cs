using UnityEngine;

public class TestAttaker : MonoBehaviour
{
    [SerializeField] private float damage = 1;
    [SerializeField] private LayerMask targetlayerMask;

    private void Start()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject hitObject = collision.gameObject;

        // Unitかどうかの判定
        UnitBase hitUnit = hitObject.GetComponent<UnitBase>();
        if (hitUnit == null) return;

        // 指定されたレイヤーマスクとの判定
        if ((targetlayerMask.value & (1 << hitObject.layer)) != 0)
        {
            string unitName = hitUnit.UnitStatusData?.name ?? hitObject.name;
            Debug.Log($"Unit [{unitName}] を検知");
            
            // ダメージ処理
            hitUnit.TakeDamage(damage);
            Debug.Log($"Unit [{unitName}] に {damage} ダメージを与えた (現在HP: {hitUnit.currentHP})");
        }
        else
        {
            Debug.Log($"Unit検知: レイヤー {LayerMask.LayerToName(hitObject.layer)} は対象外です");
        }
    }
    
    // private void OnCollisionStay2D(Collision2D collision)
    // {
    //     Debug.Log($"{collision.gameObject.name} に衝突");

    //     if (collision.gameObject.layer == layerMask)
    //     {
    //         Debug.Log($"{collision.gameObject.name} にダメージ");
    //         UnitBase target = gameObject.GetComponent<UnitBase>();
    //         target.TakeDamage(damage);
    //     }
    // }
}

using UnityEngine;

public class TestAttaker : UnitBase
{
    [SerializeField] private BulletStatus _Status;

    #region === State ===
    private enum States
    {
        none,
        idle,
        knockback
    }

    private enum Triggers
    {
        none,
        damege,
        knockbackEnd
    }

    // ステート登録
    protected override void RegisterStats()
    {
        // トランスミッショングループを作成
        var idleTrigger = new[]
        {
            (Triggers.damege, States.idle, ""),
        };
        var knockbackTrigger = new[]
        {
            (Triggers.knockbackEnd, States.knockback, ""),
        };

        // ステートマシンにStatesの移動先の追加
        _stateMachine
            .AddTransition(States.idle, idleTrigger)
            .AddTransition(States.knockback, knockbackTrigger);

        /* 待機 */
        var idle = new Idle();
        _stateMachine.AddState(States.idle, idle);

        /* ノックバック */
        var knockback = new Idle();
        _stateMachine.AddState(States.knockback, knockback);
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 指定されたレイヤーマスクとの判定
        if ((AttackLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            var go = collision.gameObject;
            if (!go.TryGetComponent<UnitBase>(out var target))
            {
                target = go.GetComponentInParent<UnitBase>();
            }

            if (target != null)
            {
                //Debug.Log($"Hit Target: {target.UnitStatusData.unitName}");
                if (target.IsInvincible)
                    return;

                UnitManager.instance.AddDamage(target, this, _Status.damage);
            }
        }
        else
        {
            Debug.Log($"Unit検知: レイヤー {LayerMask.LayerToName(collision.gameObject.layer)} は対象外です");
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

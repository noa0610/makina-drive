using UnityEngine;
using System;

/// <summary>
/// ユニットの子オブジェクト化を想定した攻撃弾丸クラス
/// </summary>
public class Attack : Bullet
{
    #region === Core Logic ===
    public override void Invoke()
    {
        // Ensure orientation matches direction when invoked
        // OrientToDirection(_direction);
    }

    public override void FixedUpdate()
    {
        _elapsedTime += Time.deltaTime;
        UpdateLifetime();
    }


    protected override void UpdateLifetime()
    {
        if (_elapsedTime >= _status.time)
        {
            NotifyDestoy();
        }
    }
    #endregion

    #region === Collision Handling ===
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        HitCheck(collision);
    }

    protected override void HitCheck(Collider2D collision)
    {
        int layerBit = 1 << collision.gameObject.layer;
        if (collision.TryGetComponent<PlatformEffector2D>(out var effector) && effector.useOneWay)
        {
            // 一方通行のプラットフォームは下からの衝突を無視
            Vector2 contactPoint = collision.ClosestPoint(transform.position);
            if (contactPoint.y < transform.position.y)
                return;
        }
        // 常に衝突可能なレイヤー
        if ((_canHitLayer.value & layerBit) != 0)
        {
            Hitted_Another(collision);
            return;
        }

        // 対象レイヤー
        if ((_targetLayer.value & layerBit) != 0)
        {
            Hitted_Target(collision);
            return;
        }
    }

    protected override void Hitted_Another(Collider2D collision)
    {
        // 「Throughable」レイヤーは貫通
        if (LayerMask.NameToLayer("Throughable") == collision.gameObject.layer)
            return;

        if (Hit()) Destroy(gameObject);
    }
    #endregion

    #region === Destroy & Cleanup ===
    protected override void OnDestroy()
    {

    }
    #endregion
}

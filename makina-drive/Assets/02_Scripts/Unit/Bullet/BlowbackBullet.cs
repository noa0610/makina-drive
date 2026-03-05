using System.Runtime.InteropServices;
using UnityEngine;

public class BlowbackBullet : Bullet
{
    [Header("吹き飛ばしダメージ用設定")]
    [Tooltip("ここで設定した速度より遅いとダメージ無し")]
    [SerializeField] private float _minDamageSpeed = 3.0f;

    [Tooltip("ここで設定した速度を超えるとダメージ無し(連鎖抑制用)")]
    [SerializeField] private float _maxDamageSpeed = 50.0f;
    private float _initialSpeed;
    private float _baseKnockbackForce;

    public void SetupBlowback(UnitBase parent, float baseForce, float initialSpeed, BulletStatus status)
    {
        _parent = parent;
        _baseKnockbackForce = baseForce;
        _initialSpeed = initialSpeed;
        _status = status;
    }

    protected new virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (_parent == null || _initialSpeed <= 0) return;

        // 現在のユニットの速度を取得
        float currentSpeed = _parent.Rigidbody2D.linearVelocity.magnitude;

        // 遅すぎるor早すぎる場合ダメージを発生させない
        if (currentSpeed < _minDamageSpeed || currentSpeed > _maxDamageSpeed) return;

        if (collision.TryGetComponent<UnitBase>(out var target))
        {
            // 自分自身は除外
            if (target == _parent) return;

            // 同じタグのユニットを攻撃する
            if (target.statusManager.ReadUnitTag() == _parent.statusManager.ReadUnitTag())
            {
                // 自身の移動速度の割合からノックバック力を計算
                float speedRatio = Mathf.Clamp01(currentSpeed / _initialSpeed);
                float chainForce = _baseKnockbackForce + speedRatio;

                // 進行方向
                Vector2 pushDir = _parent.Rigidbody2D.linearVelocity.normalized;

                // ダメージ
                UnitManager.instance.AddDamage(target, _parent, _status.damage, pushDir, chainForce, _status);

                // ヒットエフェクト
                if (EffectManager.instance != null && !string.IsNullOrEmpty(_hitEffectName))
                    EffectManager.instance.Play(_hitEffectName, target.transform.position);

                // ヒットSE
                if (SoundManager.instance != null && _hitSE.SEName != null)
                {
                    Debug.Log("BulletHit PlaySE");
                    SoundManager.instance.PlaySE(_hitSE.SEName, _hitSE.Volume);
                }

                // ヒット回数カウント
                if (Hit()) NotifyDestoy();
            }
        }
    }
}

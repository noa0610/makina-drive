using UnityEngine;
using System;
using Unity.VisualScripting;

/// <summary>
/// 弾丸の基本クラス（Trigger Collider 必須）
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    #region === Inspector ===
    [Header("Collision Layers")]
    [SerializeField, Tooltip("常に衝突可能なレイヤー")]
    protected LayerMask _canHitLayer;
    [SerializeField, Tooltip("初期方向")]
    protected Vector2 _direction;
    #endregion

    #region === Fields ===
    protected LayerMask _targetLayer;
    protected BulletStatus _status;
    protected ObjectHitCounter _hitCounter = new ();
    protected float _currentHP;
    protected float _elapsedTime;   // 経過時間
    protected float KnockbackForce; // ノックバック威力
    protected UnitBase _parent;
    #endregion

    #region === Properties ===
    public Transform Transform => transform;
    public UnitBase Parent => _parent;
    public LayerMask TargetLayer { get => _targetLayer; set => _targetLayer = value; }
    public bool CanSelfMove = true;
    public bool isParentDeadBulleDestroy = false; // 発射したユニットが消えたら弾を削除
    public float Damage => _status.damage;
    public float Knockback;
    #endregion

    #region === Events ===
    protected event Action<Bullet> OnDestoryHandle;
    #endregion

    #region === Setup & Initialization ===
    /// <summary>
    /// 弾丸のステータスをセット（生成時に呼ばれる）
    /// </summary>
    public void SetBulletStatus(BulletData bullet, LayerMask targetLayer)
    {
        _status = bullet.originalstatus;
        _currentHP = _status.hp;
        _targetLayer = targetLayer;
        _elapsedTime = 0f;  // 経過時間をリセット
        Knockback = _status.knockback;
    }
    public void SetDirection(Vector2 dir) { _direction = dir; OrientToDirection(dir); }
    public void SetParent(UnitBase parent) => _parent = parent;
    public void SetCanselfMode(bool canCanself) => CanSelfMove = canCanself;
    public void SetKnockbackForce(float multiplier)
    {
        KnockbackForce = Knockback * multiplier;
    }

    public void Reflect() { _direction = -_direction; OrientToDirection(_direction); }
    #endregion

    #region === Core Logic ===
    public virtual void Invoke()
    {
        // Ensure orientation matches direction when invoked
        OrientToDirection(_direction);
    }

    public virtual void FixedUpdate()
    {
        if (CanSelfMove)
        {
            Move(Time.deltaTime);
        }
        _elapsedTime += Time.deltaTime;
        UpdateLifetime();

        // 発射したユニットが消えたときに弾を消去
        if(_parent == null && isParentDeadBulleDestroy)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void Move(float deltaTime)
    {
        transform.position += (Vector3)_direction * _status.speed * deltaTime;
    }

    protected virtual void UpdateLifetime()
    {
        if (_elapsedTime >= _status.time)
        {
            NotifyDestoy();
        }
    }

    protected bool Hit()
    {
        if (_currentHP != -1)
        {
            _currentHP--;
            if (_currentHP < 0)
            {
                _currentHP = 0;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region === Collision Handling ===
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        HitCheck(collision);
    }

    protected virtual void HitCheck(Collider2D collision)
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

    protected virtual void Hitted_Another(Collider2D collision)
    {
        // 「Throughable」レイヤーは貫通
        if (LayerMask.NameToLayer("Throughable") == collision.gameObject.layer)
            return;

        if (Hit()) Destroy(gameObject);
    }

    protected virtual void Hitted_Target(Collider2D collision)
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

            // ヒット数の取得（0以下は1として判定）
            int limit = _status.maxHitsPerUnit > 0 ? _status.maxHitsPerUnit : 1;
            float delay = _status.hitDelayTime;

            if (!_hitCounter.TryRegisterHit(target.gameObject, limit, delay))
            {
                // 規定回数ヒットしたかディレイ中
                return;
            }

            // ノックバック威力の計算
            KnockbackForce = KnockbackForce - target.statusManager.ReadValue(Status.knockbackResistance);

            // 吹き飛ばす方向
            Vector2 pushdir = (target.transform.position - transform.position).normalized;

            UnitManager.instance.AddDamage(target, _parent, _status.damage, pushdir, KnockbackForce);
            // Debug.Log($"parent:{_parent.name}");


            if (Hit()) NotifyDestoy();
        }
    }
    #endregion

        #region === Destroy & Cleanup ===
    public virtual void NotifyDestoy()
    {
        if (OnDestoryHandle != null)
            OnDestoryHandle(this);
        else if (gameObject != null)
            Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {

    }

    public void Dispose()
    {
        OnDestoryHandle = null;
    }
    #endregion

    // （1,0）が右向きになるようにスプライトの向きを調整
    protected void OrientToDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f) return;
        transform.right = new Vector3(dir.x, dir.y, 0f);
    }
}

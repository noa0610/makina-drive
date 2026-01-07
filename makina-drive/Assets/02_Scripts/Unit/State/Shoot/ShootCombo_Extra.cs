using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// メインの攻撃と同時にもう一発の別の弾（遠距離攻撃など）を発射するコンボステート
/// </summary>
public class ShootCombo_Extra : ShootCombo
{
    [Header("追加攻撃設定")]
    [SerializeField] protected BulletData _extraBulletData;
    [SerializeField] protected float _extraCreatePos = 0.5f; // 追加弾の生成位置オフセット

    protected Bullet _instantiatedExtraBullet;

    public ShootCombo_Extra(BulletData mainData, BulletData extraData, LayerMask targetLayer, string lazeChange, string comboChange = null ) 
        : base(mainData, targetLayer, lazeChange, comboChange)
    {
        _extraBulletData = extraData;
    }

    public ShootCombo_Extra() : base() { }

    protected override async UniTask Shoot(UnitBase parent)
    {
        await base.Shoot(parent);

        ShootExtra(parent);
    }

    /// <summary>
    /// 追加攻撃の生成メソッド
    /// </summary>
    protected virtual void ShootExtra(UnitBase parent)
    {
        if (_extraBulletData == null || _extraBulletData.prefab == null) return;

        // 生成位置の計算（メインより少し前や横にずらすなどの調整が可能）
        Vector3 spawnPos = _muzzle.transform.position + new Vector3(parent.AttackDirection.x, parent.AttackDirection.y) * _extraCreatePos;

        _instantiatedExtraBullet = GameObject.Instantiate(_extraBulletData.prefab, spawnPos, Quaternion.identity);
        _instantiatedExtraBullet.CanSelfMove = true;
        _instantiatedExtraBullet.isParentDeadBulleDestroy = false;
        // 追加攻撃の初期化
        InitBulletExtra(_instantiatedExtraBullet, parent.AttackDirection, parent);
        
        Debug.Log($"Extra Attack Fired: {_extraBulletData.originalstatus.speed}");
    }

    protected void InitBulletExtra(Bullet bullet, Vector3 dict, UnitBase parent)
    {
        bullet.SetBulletStatus(_extraBulletData, _targetLayer);
        bullet.SetDirection(dict);
        bullet.SetParent(parent);
        bullet.Invoke();
        bullet.SetKnockbackForce(parent.statusManager.ReadValue(Status.knockbackMultiplier));
    }

    public void SetExtraBullet(BulletData extraBulletData)
    {
        _extraBulletData = extraBulletData;
    }

    public void SetExtraCreatePos(float extraCreatePos)
    {
        _extraCreatePos = extraCreatePos;
    }
}

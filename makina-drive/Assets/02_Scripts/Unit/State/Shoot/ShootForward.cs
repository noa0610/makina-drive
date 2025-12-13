using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 前方射撃ステート
/// </summary>
[Serializable]
public class ShootForward : ShootStateBase
{
    public ShootForward(BulletData data, LayerMask targetLayer) : base(data, targetLayer) { }
    public ShootForward() : base() { }

    protected override async UniTask Shoot(UnitBase parent)
    {
        var b = _data.prefab;
        if (b == null)
        {
            Debug.Log("Do not set bullet.");
        }
        // 弾の生成位置
        Vector3 spawnPos = _muzzle.transform.position + new Vector3(parent.AttackDirection.x, parent.AttackDirection.y) * _createPos;
        // 弾を生成
        Bullet instantiatedBullet = GameObject.Instantiate(b, spawnPos, Quaternion.identity);
        float angle = Mathf.Atan2(parent.AttackDirection.y, parent.AttackDirection.x) * Mathf.Rad2Deg;
        instantiatedBullet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        InitBullet(instantiatedBullet, parent.AttackDirection);
        await base.Shoot(parent);
    }
}

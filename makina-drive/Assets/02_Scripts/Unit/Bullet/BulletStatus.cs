using System;

[Serializable]
public struct BulletStatus
{
    public float hp;          // 弾の耐久値
    public float time;        // 存在時間（寿命）
    public float damage;      // ダメージ量
    public float speed;       // 弾速
}
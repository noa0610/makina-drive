using System;

[Serializable]
public struct BulletStatus
{
    public float hp;           // 弾の耐久値
    public float time;         // 存在時間（寿命）
    public float damage;       // ダメージ量
    public float speed;        // 弾速
    public float knockback;    // ノックバック力
    public int maxHitsPerUnit; // １体につき何回までヒットするか（多段ヒット数）
    public float hitDelayTime; // 再ヒットまでの待機時間（秒）
}
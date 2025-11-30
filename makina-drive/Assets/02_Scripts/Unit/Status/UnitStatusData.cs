using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/UnitStatus")]
public class UnitStatusData : ScriptableObject
{
    public int id;                // ID
    public string unitName;       // 名前
    public string description;    // 説明
    public float maxHp;           // 最大体力
    public float hp;              // 体力
    public float atk;             // 攻撃力
    public float def;             // 防御力
    public float speed;           // 速度
    public float collectionRange; // 収集範囲
    public float stamina;         // スタミナ
    public float knockbackPower;  // ノックバック力
    public float damageTakeScale = 1f; // 受けるダメージの割合（2なら2倍の被ダメージ）
    public UnitTags tags;
}

using UnityEngine;

public struct UnitStatus
{
    public int id;
    public string name;
    public string description;
    public float maxHp;
    public float hp;
    public float atk;
    public float def;
    public float speed;
    public float collectionRange;
    public float stamina;
    public float knockbackPower;
    public int Lv;
    public Vector2 direction; // 向き（2Dベクトル）
    public UnitTags tags;

    public UnitStatus(int id, string name, string description, float maxHp, float hp, float atk, float def, float speed, int Lv, float collectionRange, float stamina, float knockbackPower, Vector2 direction, UnitTags tags)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.maxHp = maxHp;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.speed = speed;
        this.collectionRange = collectionRange;
        this.stamina = stamina;
        this.knockbackPower = knockbackPower;
        this.Lv = Lv;
        this.direction = new Vector2(1, 0); // 初期設定
        this.tags = tags;
    }
}

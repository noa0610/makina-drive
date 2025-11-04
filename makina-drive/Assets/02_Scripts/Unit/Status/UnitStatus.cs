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
    public int Lv;
    public Vector2 direction; // 向き（2Dベクトル）

    public UnitStatus(int id, string name, string description, float maxHp, float hp, float atk, float def, int Lv, float speed,Vector2 direction)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.maxHp = maxHp;
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.speed = speed;
        this.Lv = Lv;
        this.direction = new Vector2(1, 0); // 初期設定
    }
}

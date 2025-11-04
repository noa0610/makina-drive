using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/UnitStatus")]
public class UnitStatusData : ScriptableObject
{
    public int id;
    public string unitName;
    public string description;
    public float maxHp;
    public float hp;         // 体力
    public float speed;
}

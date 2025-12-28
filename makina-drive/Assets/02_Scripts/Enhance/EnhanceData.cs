using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/EnhanceStatus")]
public class EnhanceData : ScriptableObject
{
    public string enhanceName;            // 項目名
    [TextArea] public string discription; // 説明

    public EnhanceType type;

    // ステータス強化用
    public Status targetStatus;
    public float ratioPerLevel;

    // HP回復用
    public float healRatio;
    public int maxLevel = 0;
}

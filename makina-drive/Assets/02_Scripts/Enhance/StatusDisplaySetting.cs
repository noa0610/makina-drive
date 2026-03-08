using UnityEngine;

public enum DisplayValueType { Flat, Percent }

/// <summary>
/// ステータス表示情報
/// </summary>
[System.Serializable]
public class StatusDisplaySetting
{
    public Status targetStatus;    // ステータス
    public string displayName;     // 表示名
    public Sprite icon;            // アイコン
    public DisplayValueType type;  // %表記か、固定値表記か

    [Tooltip("値が減少した場合に『強化（メリット）』と判定するか")]
    public bool isInvertedBenefit; // 反転フラグ
}

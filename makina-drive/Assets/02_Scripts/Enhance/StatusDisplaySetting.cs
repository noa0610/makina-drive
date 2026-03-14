using UnityEngine;

public enum DisplayValueType { Flat, Percent }
public enum DisplaySource {StatusManager, EnhancementAccumulation }

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

    public DisplaySource displaySource = DisplaySource.StatusManager; // ステータス値を見るか、強化累積値を見るか
}

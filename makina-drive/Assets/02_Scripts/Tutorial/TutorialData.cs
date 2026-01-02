using UnityEngine;

[CreateAssetMenu(menuName = "MakinaDrive/TutorialData")]
public class TutorialData : ScriptableObject
{
    public string windowText;
    public string centerText;
    public string subText;
    public TutorialConditionType conditionType;
    public int targetCount;
    public Transform targetPoint;  // 移動先（必要な場合）
    public IState targetState;     // Freya.Statesのどの状態をカウントするか
}

public enum TutorialConditionType 
{ 
    MoveToArea, 
    PerformAction, 
    AttackEnemy
}
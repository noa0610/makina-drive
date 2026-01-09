using System;
using UnityEngine;

/// <summary>
/// チュートリアルのステップの情報をまとめるクラス
/// </summary>
[CreateAssetMenu(menuName = "MakinaDrive/TutorialStepData")]
public class TutorialStepData : ScriptableObject
{
    [Header("表示テキスト")]
    [TextArea] public string windowText; // ウィンドウ説明文
    public string centerText;            // 現在の目的
    public string subText;               // 操作説明

    [Header("進行条件 (Task)")]
    public TutorialConditionType conditionType;
    public int taskCount;           // 必要回数
    public Vector3[] targetPoint;     // 移動先
    public string[] targetStateTag; // ユニットステートタグ

    [Header("フラグ設定")]
    public bool showExplanationWindow; // 説明ウィンドウを表示するか
    public bool stopGameDuringWindow;       // チュートリアル表示でゲーム時間を停止するか
    public bool isInvincible = false;  // このステップ中、プレイヤーを無敵にするか
}
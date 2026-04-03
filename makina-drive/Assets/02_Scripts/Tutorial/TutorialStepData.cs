using System;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// チュートリアルのステップの情報をまとめるクラス
/// </summary>
[CreateAssetMenu(menuName = "MakinaDrive/TutorialStepData")]
public class TutorialStepData : ScriptableObject
{
    [Header("表示テキスト")]
    [TextArea] public string windowText; // ウィンドウ説明文
    [TextArea] public string centerText; // 現在の目的
    public string subText;               // 操作説明

    [Header("動画表示")]
    public VideoClip tutorialVideo;      // ステップごとの動画。nullなら表示しない。

    [Header("進行条件 (Task)")]
    public TutorialConditionType conditionType;
    public int taskCount;            // 必要回数
    public Vector3[] targetPoint;    // 移動先
    public string[] targetStateTag;  // ユニットのステートタグ
    public string targetAttackTag;   // 攻撃のタグ
    public string targetUnitName;    // ユニットの名前
    public UnitTags targetUnitTag;   // ユニットのタグ

    [Header("敵スポーン情報")]
    public UnitSpawnInfo spawneEnemy;

    [Header("フラグ設定")]
    public bool showExplanationWindow;     // 説明ウィンドウを表示するか
    public bool stopGameDuringWindow;      // チュートリアル表示でゲーム時間を停止するか
    public bool isInvincible = false;      // このステップ中、プレイヤーを無敵にするか
    public bool showFailureWindow = false; // 失敗時の演出を表示するか
}
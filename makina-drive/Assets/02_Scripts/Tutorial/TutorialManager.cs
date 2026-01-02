using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

/// <summary>
/// チュートリアル管理用クラス
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [SerializeField] private List<TutorialData> steps;
    [SerializeField] private Freya player;
    private IntReactiveProperty currentCount = new IntReactiveProperty();
    private int stepIndex = 0;

    void Start()
    {
        player.SetInvincible(true); // チュートリアル中は無敵
        SetupStep(0);
    }

    private void SetupStep(int index)
    {
        var step = steps[index];
        currentCount.Value = step.targetCount;

        // UIの更新や説明パネルの表示（ここでPause）
        // ShowExplainingPanel(step.windowText);

        // 条件監視の開始
        if (step.conditionType == TutorialConditionType.PerformAction)
        {
            // StateMachineの現在の状態を監視
            // player.stateMachine.OnStateChanged // 状態変更イベントをStateMachineに追加しておくと便利
                // .Where(s => s.Name == step.targetState.ToString())
                // .Subscribe(_ => OnActionPerformed());
        }
    }

    private void OnActionPerformed()
    {
        currentCount.Value--;
        // if (currentCount.Value <= 0) NextStep();
    }
}

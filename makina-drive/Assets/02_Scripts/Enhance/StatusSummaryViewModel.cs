using UnityEngine;

public enum StatusChangeState { None, Positive, Negative }

public class StatusSummaryViewModel
{
    public Sprite icon;
    public string name;
    public string currentValueText;
    public string diffValueText;         // "+" とか "120%" といった差分表記用

    public bool isEnhanced;               // 強化されている場合に色を変える
    public StatusChangeState changeState; // 増加・減少の状態
    public bool isInvertedBenefit;        // メリット判定を反転させるか

    // 「プレイヤーにとって得か損か」を返すプロパティ
    public bool IsBenefit => changeState == StatusChangeState.None ? false : 
                             (changeState == StatusChangeState.Positive ? !isInvertedBenefit : isInvertedBenefit);
}

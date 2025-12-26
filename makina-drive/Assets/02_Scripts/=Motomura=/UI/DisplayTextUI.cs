using TMPro;
using UnityEngine;
using makinadrive.MT.UI;
using UnityEngine.UI;
using DG.Tweening;


public class DisplayTextUI : MonoBehaviour
{
/*============================================================================*/

    [SerializeField,Header("Player Status Scriptable Object")]
    private PlayerStatus _playerStatus;

    [SerializeField,Header("Level Up Window")]
    private LevelUpWindowUI _LevelUpWindowUI;

    [SerializeField,Header("HP")] 
    private TextMeshProUGUI _HPText;
    [SerializeField]
    private Image _HPBar;


    [SerializeField,Header("Level")]
    // private TextMeshProUGUI _LevelText;
    // [SerializeField]
    private Image _LevelBar;

    [SerializeField,Header("Silver Coin")]
    private TextMeshProUGUI _SilverCoinText;

    [SerializeField,Header("Gold Coin")]
    private TextMeshProUGUI _GoldCoinText;

/*============================================================================*/


/*============================================================================*/

void Start()
    {
        //初期設定  多分、スクリプタブルオブジェクトから取ってくる

        DisplayHP(_playerStatus.currentHP, _playerStatus.maxHP);
        DisplayEXP(_playerStatus.currentEXP, _playerStatus.maxEXP);
        DisplaySilverCoin(_playerStatus.silverCoin);
        DisplayGoldCoin(_playerStatus.goldCoin);
    }
/*============================================================================*/

    void OnEnable()
    {
        UIEventService.HP += DisplayHP;
        UIEventService.EXP += DisplayEXP;
        UIEventService.SilverCoin += DisplaySilverCoin;
        UIEventService.GoldCoin += DisplayGoldCoin;
    }
    void OnDisable()
    {
        UIEventService.HP -= DisplayHP;
        UIEventService.EXP -= DisplayEXP;
        UIEventService.SilverCoin -= DisplaySilverCoin;
        UIEventService.GoldCoin -= DisplayGoldCoin;
    }

/*============================================================================*/

public void DisplayHP(int newHP, int maxHP)
    {
        if (newHP < 0) newHP = 0;
        if (newHP > maxHP) newHP = maxHP;
        _HPText.SetText(newHP + "/" + maxHP); 
        CauntUP("HP", newHP, maxHP);
    }

    void DisplayEXP(int exp, int maxEXP)
    {
        // _LevelText.SetText("Lv." + level.ToString("d2"));
        CauntUP("EXP", exp, maxEXP);
        Debug.Log(exp + " / " + maxEXP);
    }

    void DisplaySilverCoin(int silverCoin)
    {
        _SilverCoinText.SetText(silverCoin.ToString("d5"));
    }

    void DisplayGoldCoin(int goldCoin)
    {
        _GoldCoinText.SetText(goldCoin.ToString("d5"));
    }

    void CauntUP(string Status , int Num , int MaxNum)
    {
        
        switch (Status)
        {
            case "HP":
            var currentHPFillAmount = (float)Num / MaxNum;
                _HPBar.DOFillAmount(currentHPFillAmount, 1).SetEase(Ease.OutCubic);
                break;
            case "EXP":
            var currentLevelFillAmount = (float)Num / MaxNum;
            if (Num >= MaxNum)
                {
                    currentLevelFillAmount = 0f;
                    _LevelBar.DOFillAmount(currentLevelFillAmount, 0.2f).SetEase(Ease.OutCubic);
                    _LevelUpWindowUI.OpenLevelUpWindow();
                }

                _LevelBar.DOFillAmount(currentLevelFillAmount, 0.2f).SetEase(Ease.OutCubic);
                break;
        }
        
    }

}

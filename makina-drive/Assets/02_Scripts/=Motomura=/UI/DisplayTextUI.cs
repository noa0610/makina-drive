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

    [SerializeField,Header("HP")] 
    private TextMeshProUGUI _HPText;
    [SerializeField]
    private Image _HPBarHealth;
    [SerializeField]
    private Image _HPBarLow;


    [SerializeField,Header("Level")]
    private TextMeshProUGUI _LevelText;
    [SerializeField]
    private Image _LevelBarHigh;

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
        DisplayLevel(_playerStatus.level);
        DisplaySilverCoin(_playerStatus.silverCoin);
        DisplayGoldCoin(_playerStatus.goldCoin);
    }
/*============================================================================*/

    void OnEnable()
    {
        UIEventService.HP += DisplayHP;
        UIEventService.Level += DisplayLevel;
        UIEventService.SilverCoin += DisplaySilverCoin;
        UIEventService.GoldCoin += DisplayGoldCoin;
    }
    void OnDisable()
    {
        UIEventService.HP -= DisplayHP;
        UIEventService.Level -= DisplayLevel;
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

    void DisplayLevel(int level)
    {
        _LevelText.SetText("Lv." + level.ToString("d2"));
        CauntUP("Level", level, 100);
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
            var currentFillAmount = (float)Num / MaxNum;
                _HPBarHealth.DOFillAmount(currentFillAmount, 0).SetEase(Ease.OutCubic);
                _HPBarLow.DOFillAmount(currentFillAmount, 0.5f).SetEase(Ease.OutCubic).SetDelay(0.5f);
                break;
            case "Level":

                //_LevelBarHigh.DOFillAmount(currentFillAmount, 0).SetEase(Ease.OutCubic);
                break;
        }
        
    }

}

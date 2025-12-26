using System;

namespace makinadrive.MT.UI
{
    public class UIEventService
    {

/*============================================================================*/

        public static Action<int, int> HP;
        public static Action<int, int> EXP;
        public static Action<int> SilverCoin;
        public static Action<int> GoldCoin;

/*============================================================================*/

        public static void UpdateHP(int NewHP, int MaxHP)
        {
            HP?.Invoke(NewHP, MaxHP);
        }

        public static void UpdateEXP(int NewEXP, int MaxEXP)
        {
            EXP?.Invoke(NewEXP, MaxEXP);
        }

        public static void UpdateSilverCoin(int NewSilverCoin)
        {
            SilverCoin?.Invoke(NewSilverCoin);
        }

        public static void UpdateGoldCoin(int NewGoldCoin)
        {
            GoldCoin?.Invoke(NewGoldCoin);
        }

/*============================================================================*/
    }
}

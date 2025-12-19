using System;

namespace makinadrive.MT.UI
{
    public class UIEventService
    {

/*============================================================================*/

        public static Action<int, int> HP;
        public static Action<int> Level;
        public static Action<int> SilverCoin;
        public static Action<int> GoldCoin;

/*============================================================================*/

        public static void UpdateHP(int NewHP, int MaxHP)
        {
            HP?.Invoke(NewHP, MaxHP);
        }

        public static void UpdateLevel(int NewLevel)
        {
            Level?.Invoke(NewLevel);
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

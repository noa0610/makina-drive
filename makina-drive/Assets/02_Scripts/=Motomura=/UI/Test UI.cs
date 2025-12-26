using UnityEngine;
using makinadrive.MT.UI;

public class TestUI : MonoBehaviour
{
    int _EXP = 0;
    // [SerializeField]
    // int _AttackHP = 100;
    public void Test_EXPUp()
    {  
        if(_EXP >= 100)
        {
            _EXP = 0;
            UIEventService.UpdateEXP(0, 100);
        } 
        UIEventService.UpdateEXP(_EXP+=10, 100);

        Debug.Log(_EXP);
    }


}

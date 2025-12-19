using UnityEngine;
using makinadrive.MT.UI;

public class TestUI : MonoBehaviour
{
    int _hp = 100;
    [SerializeField]
    int _AttackHP = 100;
    public void Test_LevelUp()
    {   
        UIEventService.UpdateHP(_hp += _AttackHP, 100);
    }
}

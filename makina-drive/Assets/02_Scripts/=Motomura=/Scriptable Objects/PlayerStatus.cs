using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "MT/PlayerStatus")]
public class PlayerStatus : ScriptableObject
{
    public int maxHP = 100;
    public int currentHP = 100;
    public int level = 1;
    public int silverCoin = 0;
    public int goldCoin = 0;
}

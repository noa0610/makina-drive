using UnityEngine;
[CreateAssetMenu(menuName = "MakinaDrive/BulletData")]
public class BulletData : ScriptableObject
{
    public string bulletName;
    public Bullet prefab;
    public BulletStatus originalstatus;
}

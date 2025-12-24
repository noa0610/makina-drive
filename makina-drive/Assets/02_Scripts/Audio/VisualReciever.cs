using UnityEngine;

/// <summary>
/// UnitBaseのPlaySEを起動するクラス
/// </summary>
public class VisualReciever : MonoBehaviour
{
    [SerializeField] protected UnitBase _target;
    [SerializeField] private VisualInfo _visualInfo;

    public virtual void PlayNormalShootSE()
    {
        //AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>(_normalShoot.SEName), transform.position, _normalShoot.Volume);
    }

    public void Play(VisualInfo info)
    {
        _target.PlaySE(info.SEName, info.Volume);
    }


}

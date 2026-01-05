using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private VisualInfo _visualInfo;
    [SerializeField] private bool _loopPlayback = false;

    private void Start()
    {
        SoundManager.instance.PlayBGM(_visualInfo.SEName, _visualInfo.Volume, _loopPlayback);
    }

    private void OnEnable()
    {
        SoundManager.instance.StopBGM(_visualInfo.SEName);
    }
}

using UnityEngine;

/// <summary>
/// BGM再生機能
/// </summary>
public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private VisualInfo _visualInfo;
    [SerializeField] private bool _loopPlayback = false;
    public bool _stopBGM = false;      // trueで再生停止、falseで再生再開
    private bool _isPlayBGM = false;

    private void Start()
    {
        if (_stopBGM || string.IsNullOrEmpty(_visualInfo.SEName)) return;

        SoundManager.instance.PlayBGM(_visualInfo.SEName, _visualInfo.Volume, _loopPlayback);
        _isPlayBGM = true;
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(_visualInfo.SEName)) return;

        if (_stopBGM && _isPlayBGM)
        {
            SoundManager.instance.StopBGM(_visualInfo.SEName);
            _isPlayBGM = false;
        }
        else if (!_stopBGM && !_isPlayBGM)
        {
            SoundManager.instance.PlayBGM(_visualInfo.SEName, _visualInfo.Volume, _loopPlayback);
            _isPlayBGM = true;
        }
    }

    private void OnEnable()
    {
        SoundManager.instance.StopBGM(_visualInfo.SEName);
    }
}

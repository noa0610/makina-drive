using UnityEngine;

/// <summary>
/// BGM再生機能
/// </summary>
public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private VisualInfo _visualInfo;
    [SerializeField] private bool _loopPlayback = true;
    public bool _stopBGM = false;      // trueで再生停止、falseで再生再開
    private bool _isPlayBGM = false;
    private string _currentPlayingName = ""; // 現在コンポーネント指定しているBGM名

    private void Start()
    {
        TryPlay();
    }

    private void Update()
    {
        if (_stopBGM)
        {
            if (string.IsNullOrEmpty(_visualInfo.SEName))
            {
                SoundManager.instance.StopBGM(_visualInfo.SEName);
                _currentPlayingName = "";
            }
            return;
        }

        if (!string.IsNullOrEmpty(_visualInfo.SEName) && _currentPlayingName != _visualInfo.SEName)
        {
            TryPlay();
        }
    }

    private void TryPlay()
    {
        if (_stopBGM || string.IsNullOrEmpty(_visualInfo.SEName)) return;
        if (SoundManager.instance == null) return;

        SoundManager.instance.PlayBGM(_visualInfo.SEName, _visualInfo.Volume, _loopPlayback);
        _currentPlayingName = _visualInfo.SEName;
    }
}

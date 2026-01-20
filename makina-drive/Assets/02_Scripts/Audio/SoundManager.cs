using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System.IO;

/// <summary>
/// Resourcesフォルダ内のBGM、SEフォルダにある音源素材を名前指定で再生するシングルトン
/// </summary>
public class SoundManager : SingletonBehavior<SoundManager>
{
    [System.Serializable]
    private class SoundData
    {
        public SoundData(AudioClip audioClip)
        {
            this.audioClip = audioClip;
            playedTime = -100;
        }

        public AudioClip audioClip;
        public float playedTime; //再生時間
    }


    [SerializeField] private float INTERVAL = 0.2f; // 一度再生してから、次再生出来るまでの間隔(秒)
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _bgmGroup;
    [SerializeField] private AudioMixerGroup _seGroup;
    private SoundSettings _settings = new SoundSettings();
    private string _savePath;

    // AudioSource（スピーカー）を同時に鳴らしたい音の数だけ用意
    private AudioSource[] _seSources = new AudioSource[20];
    private AudioSource[] _bgmSources = new AudioSource[1];

    private Dictionary<string, SoundData> _seData = new Dictionary<string, SoundData>();
    private Dictionary<string, SoundData> _bgmData = new Dictionary<string, SoundData>();

    protected override void Awake()
    {
        base.Awake();

        if (instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            return; // 重複インスタンスなら以下の処理は不要
        }

        _savePath = Path.Combine(Application.persistentDataPath, "sound_settings.json");
        LoadSettings();

        //AudioSourceを自分自身に生成して配列に格納
        for (int i = 0; i < _seSources.Length; i++)
        {
            _seSources[i] = gameObject.AddComponent<AudioSource>();
            _seSources[i].outputAudioMixerGroup = _seGroup;
        }

        for (int i = 0; i < _bgmSources.Length; i++)
        {
            _bgmSources[i] = gameObject.AddComponent<AudioSource>();
            _bgmSources[i].outputAudioMixerGroup = _bgmGroup;
        }

        //音データの読み込み
        var seClips = Resources.LoadAll<AudioClip>("SE");
        var bgmClips = Resources.LoadAll<AudioClip>("BGM");

        //音データの格納
        foreach (var seClip in seClips)
        {
            var soundData = new SoundData(seClip);
            _seData.Add(seClip.name, soundData);
        }

        foreach (var bgmClip in bgmClips)
        {
            var soundData = new SoundData(bgmClip);
            _bgmData.Add(bgmClip.name, soundData);
        }
    }

    private void Start()
    {
        SetBGMVolume(_settings.bgmVolume);
        SetSEVolume(_settings.seVolume);
    }

    public void SetBGMVolume(float volume)
    {
        _settings.bgmVolume = volume;
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f; // 未定義防止
        _audioMixer.SetFloat("BGMVolume", db);
    }

    public void SetSEVolume(float volume)
    {
        _settings.seVolume = volume;
        float db = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f; // 未定義防止
        _audioMixer.SetFloat("SEVolume", db);
    }

    public SoundSettings GetSettings() => _settings;

    // json保存
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(_settings);
        File.WriteAllText(_savePath, json);
    }

    // json読み込み
    public void LoadSettings()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _settings = JsonUtility.FromJson<SoundSettings>(json);
        }
    }

    /// <summary>
    /// 未使用のAudioSource(SE)を検索し、取得する関数
    /// </summary>
    /// <returns>未使用のAudioSource(SE)。全て使用中の場合はnullを返却</returns>
    private AudioSource GetUnusedSourceSE()
    {
        for (var i = 0; i < _seSources.Length; ++i)
        {
            if (!_seSources[i].isPlaying) return _seSources[i];
        }

        return null;
    }

    /// <summary>
    /// 未使用のAudioSource(BGM)を検索し、取得する関数
    /// </summary>
    /// <returns>未使用のAudioSource(BGM)。全て使用中の場合はnullを返却</returns>
    private AudioSource GetUnusedSourceBGM()
    {
        for (var i = 0; i < _bgmSources.Length; ++i)
        {
            if (!_bgmSources[i].isPlaying) return _bgmSources[i];
        }

        return null;
    }

    /// <summary>
    /// 名前で指定したSEを再生する関数　引数で音量調節
    /// </summary>
    /// <param seName="seName">SE名</param>
    /// <param volume="volume">音量</param>
    public void PlaySE(string seName, float volume = 1f)
    {
        if (_seData.ContainsKey(seName))
        {
            //Debug.Log($"SoundManager: Playing SE '{seName}'");
            if (Time.realtimeSinceStartup - _seData[seName].playedTime > INTERVAL)
            {
                var audioSource = GetUnusedSourceSE();

                if (audioSource)
                {
                    audioSource.clip = _seData[seName].audioClip;
                    audioSource.volume = volume;
                    audioSource.Play();
                    _seData[seName].playedTime = Time.realtimeSinceStartup;
                }
                //else
                //    Debug.LogWarning("SoundManager: All SE AudioSources are in use!");
            }
        }
        //else
        //{
        //    Debug.LogWarning($"SoundManager: SE '{seName}' not found!");
        //}
    }

    /// <summary>
    /// 名前で指定したBGMを再生する関数
    /// </summary>
    /// <param bgmName="bgmName">BGM名</param>
    /// <param volume="volume">音量</param>
    public void PlayBGM(string bgmName, float volume = 1f, bool loopPlayback = false)
    {
        if (!_bgmData.TryGetValue(bgmName, out var data))
        {
            Debug.LogWarning($"BGM {bgmName} が見つかりません");
            return;
        }

        var audioSource = _bgmSources[0];

        // 同じBGM指定の場合音量のみを更新
        if (audioSource.isPlaying && audioSource.clip == data.audioClip)
        {
            audioSource.volume = volume;
            return;
        }

        Debug.Log($"BGM {bgmName} 再生開始");
        audioSource.Stop();
        audioSource.clip = data.audioClip;
        audioSource.volume = volume;
        audioSource.loop = loopPlayback;
        audioSource.Play();

        data.playedTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// BGMを停止する関数
    /// </summary>
    public void StopBGM(string bgmName)
    {
        foreach (var bgmSource in _bgmSources)
        {
            if (bgmSource.clip && bgmSource.clip.name == bgmName)
            {
                if (bgmSource.isPlaying)
                    bgmSource.Stop();
            }
        }
    }

    public void AllStopBGM()
    {
        foreach (var bgmSource in _bgmSources)
        {
            if (bgmSource.isPlaying)
                bgmSource.Stop();
        }
    }
}
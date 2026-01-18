using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// BGMとSEのスライダー操作用クラス
/// </summary>
public class VolumeController : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private TextMeshProUGUI _bgmVolumeText;

    [Header("SE Settings")]
    [SerializeField] private Slider _seSlider;
    [SerializeField] private TextMeshProUGUI _seVolumeText;

    private void Start()
    {
        var settings = SoundManager.instance.GetSettings();

        _bgmSlider.value = settings.bgmVolume;
        _seSlider.value = settings.seVolume;
        UpdateBGMText(settings.bgmVolume);
        UpdateSEText(settings.seVolume);

        _bgmSlider.onValueChanged.AddListener(val =>
        {
            SoundManager.instance.SetBGMVolume(val);
            UpdateBGMText(settings.bgmVolume);
        });

        _seSlider.onValueChanged.AddListener(val =>
        {
            SoundManager.instance.SetSEVolume(val);
            UpdateSEText(settings.seVolume);
        });

        AddPointerUpTrigger(_bgmSlider);
        AddPointerUpTrigger(_seSlider);
    }

    // テキストの更新
    private void UpdateBGMText(float value)
    {
        if (_bgmVolumeText != null)
            _bgmVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }
    private void UpdateSEText(float value)
    {
        if (_seVolumeText != null)
            _seVolumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    // EventTriggerを使って「マウス/指を離した時」のイベントを追加する
    private void AddPointerUpTrigger(Slider slider)
    {
        EventTrigger trigger = slider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = slider.gameObject.AddComponent<EventTrigger>();

        // PointerUp（離した時）のイベントエントリを作成
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => { OnPointerUpSlider(); });

        trigger.triggers.Add(entry);
    }

    // スライダーを離した際に呼ばれる処理
    private void OnPointerUpSlider()
    {
        OnCloseSettings();
        Debug.Log("Sound settings saved to JSON.");
    }

    // 設定を保存
    public void OnCloseSettings()
    {
        SoundManager.instance.SaveSettings();
    }
}

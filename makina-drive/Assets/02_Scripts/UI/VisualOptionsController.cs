using UnityEngine;
using UnityEngine.UI;

public class VisualOptionsController : MonoBehaviour
{
    [SerializeField] private Toggle _hpBarToggle;
    [SerializeField] private Toggle _damageTextToggle;
    [SerializeField] private Toggle _immediateEnhanceToggle;

    private void Start()
    {
        var manager = VisualSettingsManager.instance;
        var settings = manager.Settings;

        // 初期値の繁栄
        _hpBarToggle.isOn = settings.showHPBar;
        _damageTextToggle.isOn = settings.showDamageText;
        _immediateEnhanceToggle.isOn = settings.isImmediateEnhancement;

        // 値変更のイベント登録
        _hpBarToggle.onValueChanged.AddListener(val =>
        {
            manager.SetShowHPBar(val);
        });

        _damageTextToggle.onValueChanged.AddListener(val =>
        {
            manager.SetShowDamageText(val);
        });

        _immediateEnhanceToggle.onValueChanged.AddListener(val =>
        {
            manager.SetIsImmediateEnhancement(val);
        });
    }
}

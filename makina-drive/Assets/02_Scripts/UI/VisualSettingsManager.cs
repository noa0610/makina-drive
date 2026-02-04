using UnityEngine;
using System.IO;

public class VisualSettingsManager : SingletonBehavior<VisualSettingsManager>
{
    private VisualSettings _settings = new VisualSettings();
    private string _savePath;

    public VisualSettings Settings => _settings;

    protected override void Awake()
    {
        base.Awake();
        _savePath = Path.Combine(Application.persistentDataPath, "visualsettings.json");
        LoadSettings();
    }

    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(_settings, true);
        File.WriteAllText(_savePath, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            _settings = JsonUtility.FromJson<VisualSettings>(json);
        }
    }

    // 設定更新をするメソッド
    public void SetShowHPBar(bool value)
    {
        _settings.showHPBar = value;
        SaveSettings();
    }

    public void SetShowDamageText(bool value)
    {
        _settings.showDamageText = value;
        SaveSettings();
    }

    public void SetIsImmediateEnhancement(bool value)
    {
        _settings.isImmediateEnhancement = value;
        SaveSettings();
    }
}

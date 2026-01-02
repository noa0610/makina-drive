using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 強化項目の情報をUI表示するクラス
/// </summary>
public class EnhanceChoiceSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private Button _button;

    public void Setup(EnhanceViewModel vm, EnhanceData data, Action<EnhanceData> onClick)
    {
        _nameText.text = vm.name;                   // 項目名
        _descriptionText.text = vm.description;     // 説明
        _levelText.text = $"Lv: {vm.currentLevel}"; // 強化回数

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => onClick(data));
    }
}

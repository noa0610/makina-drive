using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusSummarySlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _diffText;

    [Header("文字色")]
    [SerializeField] private Color _benefitColor = Color.green;  // メリット時の色
    [SerializeField] private Color _penaltyColor = Color.red;    // デメリット時の色
    [SerializeField] private Color _neutralColor = Color.white;  // 変化なしの色

    public void Setup(StatusSummaryViewModel model)
    {
        if (_iconImage != null)
        {
            _iconImage.sprite = model.icon;
        }

        _nameText.text = model.name;
        _diffText.text = model.diffValueText;

        if (model.changeState == StatusChangeState.None)
        {
            _diffText.color = _neutralColor;
        }
        else
        {
            _diffText.color = model.IsBenefit ? _benefitColor : _penaltyColor;
        }
    }
}

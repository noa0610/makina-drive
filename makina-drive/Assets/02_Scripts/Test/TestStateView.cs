using TMPro;
using UnityEngine;

public class TestStateView : MonoBehaviour
{
    [SerializeField] private UnitBase _unit;
    [SerializeField] private TextMeshProUGUI _text;
    private bool _View = true;

    private void Start()
    {
        if(_unit == null || _text == null)
        {
            Debug.Log("ユニットもしくは表示するテキストが未設定です。");
            _View = false;
        }
    }

    private void Update()
    {
        if(_View == false) return;
        _text.text = _unit.stateMachine.CurrentState.Name + ":" + _unit.name;
    }
}

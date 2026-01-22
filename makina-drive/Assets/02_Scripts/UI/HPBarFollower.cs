using UnityEngine;

/// <summary>
/// ユニットのHPバーを追従させる
/// </summary>
public class HPBarFollower : MonoBehaviour
{
    private Transform _targetTransform;
    private Vector3 _offset = new Vector3(0, 1.2f, 0);
    private RectTransform _rectTransform;
    private Camera _mainCamera;

    public void SetTarget(Transform target, Vector3 offset)
    {
        _targetTransform = target;
        _offset = offset;
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // 画面外であれば非表示にする
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_targetTransform.position + _offset);
        if(screenPos.z < 0)
        {
            _rectTransform.localScale = Vector3.zero;
        }
        else
        {
            _rectTransform.localScale = Vector3.one;
            _rectTransform.position = screenPos;
        }
    }
}

using UnityEngine;

/// <summary>
/// ユニットに追従し攻撃方向に合わせて向きを変更
/// </summary>
public class DirectionIndicator : MonoBehaviour
{
    [SerializeField] private float _distance = 1.2f;
    [SerializeField] private bool _isVisible = true;

    private UnitBase _target;
    private SpriteRenderer _renderer;
    

    public void SetVisible(bool visible) => _isVisible = visible;

    public void Setup(UnitBase target, float distance, bool visible = true)
    {
        // 以前のターゲットがあれば削除
        if (_target != null)
        {
            _target.OnUnitDeath -= HandleTargetDeath;
        }

        _target = target;
        _distance = distance;
        _isVisible = visible;
        _renderer = GetComponent<SpriteRenderer>();

        // ユニット死亡時に自身を削除
        if (_target != null)
        {
            _target.OnUnitDeath += HandleTargetDeath;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        UpdateVisibility();
        if (!_isVisible) return;

        Vector2 attackDir = _target.AttackDirection;
        Vector3 targetPos = _target.transform.position;

        transform.position = targetPos + (Vector3)(attackDir.normalized * _distance);

        if (attackDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void UpdateVisibility()
    {
        if(_renderer != null && _renderer.enabled != _isVisible)
        {
            _renderer.enabled = _isVisible;
        }
    }

    private void HandleTargetDeath(UnitBase unit)
    {
        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (_target != null)
        {
            _target.OnUnitDeath -= HandleTargetDeath;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class EnemyHPSlider : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    private StatusInfo _hpInfo;
    private StatusInfo _maxHPInfo;

    public void SetUP(StatusManager statusManager)
    {
        _hpInfo = statusManager.GetStatus(Status.HP);
        _hpInfo = statusManager.GetStatus(Status.MaxHP);

        _hpInfo.OnAmountChanged += (before, after) => UpdateVisual();

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_maxHPInfo.CurrentAmount > 0)
        {
            _hpSlider.value = _hpInfo.CurrentAmount / _maxHPInfo.CurrentAmount;
        }
    }

    private void LateUpdate()
    {
        // 親が反転していてもUIの向きを正常に保つ
        Vector3 currentScale = transform.localScale;
        float parentScaleX = transform.parent.localScale.x;

        // 親が負（左向き）なら自分も負にすることで見た目反転を打ち消す
        float targetScaleX = Mathf.Abs(currentScale.x)* (parentScaleX > 0 ? 1 : -1);

        if(!Mathf.Approximately(currentScale.x, targetScaleX))
        {
            transform.localScale = new Vector3(targetScaleX, currentScale.y, currentScale.z);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}

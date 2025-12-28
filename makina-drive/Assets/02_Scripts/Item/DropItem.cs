using UnityEngine;

public abstract class DropItem : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float accel = 0.5f;
    protected UnitBase _target;
    protected bool _isAttracted = false;

    // 引き寄せられる対象をセット
    public void Setup(UnitBase player)
    {
        _target = player;
    }

    protected virtual void Update()
    {
        if (_target == null) return;

        float distance = Vector3.Distance(transform.position, _target.transform.position);

        float range = _target.statusManager.ReadValue(Status.CollectionRange);

        if (distance <= range) _isAttracted = true;

        if (_isAttracted)
        {
            // 徐々に加速しながら引き寄せ
            moveSpeed += accel;
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    // アイテム取得
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(_target == null) return;
        
        if (collision.CompareTag("Player"))
        {
            OnCollect(_target);
            Destroy(gameObject);
        }
    }

    protected abstract void OnCollect(UnitBase target);
}

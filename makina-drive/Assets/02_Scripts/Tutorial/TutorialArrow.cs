using UnityEngine;

/// <summary>
/// チュートリアル時のエリアの先を表示するクラス
/// </summary>
public class TutorialArrow : MonoBehaviour
{
    public Transform player;
    public Vector3 targetPos;

    public void SetTarget(Vector3 target)
    {
        targetPos = target;
    }

    void Update()
    {
        if (targetPos == Vector3.zero) return;
        
        Vector3 dir = targetPos - player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    // TODO 一番近いエリアを自動で指すようにする
}

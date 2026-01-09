using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// チュートリアル時のエリアの先を表示するクラス
/// </summary>
public class TutorialArrow : MonoBehaviour
{
    [SerializeField] private Transform _player;
    public List<Vector3> _activeTargetPos= new List<Vector3>();

    public void SetTargets(IEnumerable<Vector3> targets)
    {
        _activeTargetPos = new List<Vector3>(targets);
    }

    public void RemoveTarget(Vector3 reachedPos)
    {
        _activeTargetPos.Remove(reachedPos);
    }

    void Update()
    {
        if (_player == null || _activeTargetPos.Count == 0) return;
        
        Vector3 closest = _activeTargetPos[0];
        float minDistance = Vector3.Distance(_player.position, closest);

        foreach(var pos in _activeTargetPos)
        {
            float dist = Vector3.Distance(_player.position, pos);
            if(dist < minDistance)
            {
                minDistance = dist;
                closest = pos;
            }
        }

        Vector3 dir = closest - _player.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}

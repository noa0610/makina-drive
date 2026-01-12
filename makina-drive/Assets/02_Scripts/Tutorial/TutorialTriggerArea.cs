using UnityEngine;

/// <summary>
/// チュートリアル時にこのエリアを生成して、接触で進行させるクラス
/// </summary>
public class TutorialTriggerArea : MonoBehaviour
{
    private TutorialManager _manager;
    public void SetManager(TutorialManager manager)
    {
        _manager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Area in");
            _manager.OnAreaReached(transform.position);
            Destroy(gameObject);
        }
    }
}
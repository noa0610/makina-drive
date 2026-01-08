using UnityEngine;

/// <summary>
/// チュートリアル時にこのエリアを生成して、接触で進行させるクラス
/// </summary>
public class TutorialTriggerArea : MonoBehaviour
{
    [SerializeField] private TutorialManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.AddCount();
            gameObject.SetActive(false); // 1回きり
        }
    }
}
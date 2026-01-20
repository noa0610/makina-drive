using UnityEngine;
using UnityEngine.UI;

public class GameEndButton : MonoBehaviour
{
    [SerializeField] private Button _transitionButton;

    private void Start()
    {
        if (_transitionButton != null)
        {
            _transitionButton.onClick.AddListener(() =>
            {

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
    }
}

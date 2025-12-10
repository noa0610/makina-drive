using UnityEngine;
using UnityEngine.EventSystems;

public class InputButton : MonoBehaviour
{
    [SerializeField, MultilineAttribute (2)]
    string Information;
    [SerializeField]
    private GameObject _StartToGame;
    [SerializeField]
    private GameObject _StartButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(_StartToGame);
    }

    public void CompleteUI_Slide()
    {
        EventSystem.current.SetSelectedGameObject(_StartButton);
    }
}   

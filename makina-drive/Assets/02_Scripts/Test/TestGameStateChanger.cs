using UnityEngine;

public class TestGameStateChanger : MonoBehaviour
{
    [Header("Enterを押しながらEsc")]
    [SerializeField] private GameState _changeState1;

    [Header("Enterを押しながらBackspace")]
    [SerializeField] private GameState _changeState2;


    void Update()
    {
        if (Input.GetKey(KeyCode.Return) && Input.GetKeyDown(KeyCode.Escape))
        {
            GameStateManager.instance.ChangeState(_changeState1);
        }

        if (Input.GetKey(KeyCode.Return) && Input.GetKeyDown(KeyCode.Backspace))
        {
            GameStateManager.instance.ChangeState(_changeState2);
        }
    }
}

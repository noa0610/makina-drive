using UnityEngine;

public class TestGameStateChanger : MonoBehaviour
{
    [SerializeField] private GameState _changeState = GameState.Clear;
    
    void Update()
    {
        if(Input.GetKey(KeyCode.Return) && Input.GetKeyDown(KeyCode.Escape))
        {
            GameStateManager.instance.ChangeState(_changeState);
        }
    }
}

using UnityEngine;

public enum GameState
{
    Exploring, 
    Paused,  
    Map,
    Puzzle,    
    Dialogue,
    OnWhale
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CurrentState = GameState.Exploring;
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log("[GameManager] Oyun Durumu Deðiþti: " + CurrentState);

        if (newState == GameState.Exploring)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

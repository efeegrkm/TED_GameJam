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
        Application.targetFrameRate = 60;

        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CurrentState = GameState.Exploring;
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if(newState != GameState.Exploring)
        {
            PlayerMovementManager.Instance.StopMovement();
        }
        else
        {
            PlayerMovementManager.Instance.ResumeMovement();
            //EventManager.Instance.map.SetActive(false);
        }
    }
}

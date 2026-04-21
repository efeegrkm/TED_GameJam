using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private bool canInteract = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            EventManager.Instance.TogglePause();
        }

        if (!canInteract) return;

        if (GameManager.Instance.CurrentState != GameState.Exploring && GameManager.Instance.CurrentState != GameState.Map) return;

        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            EventManager.Instance.ToggleMap();
        }

        if (GameManager.Instance.CurrentState != GameState.Exploring) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            InteractionManager.Instance.TryInteract();
        }
    }
    public void StopInteraction()
    {
        canInteract = false;
    }

    public void ResumeInteraction()
    {
        canInteract = true;
    }
}
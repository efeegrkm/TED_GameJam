using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            EventManager.Instance.TogglePause();
        }

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
}
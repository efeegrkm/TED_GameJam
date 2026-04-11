using UnityEngine;
using Unity.Cinemachine; 

public class CameraStateController : MonoBehaviour
{
    private CinemachineInputAxisController cinemachineInput;

    void Start()
    {
        cinemachineInput = GetComponent<CinemachineInputAxisController>();
    }

    void Update()
    {
        if (cinemachineInput == null) return;

        if (GameManager.Instance.CurrentState == GameState.Exploring)
        {
            cinemachineInput.enabled = true;
        }
        else
        {
            cinemachineInput.enabled = false;
        }
    }
}
using UnityEngine;

public class GhostAnimator : MonoBehaviour
{
    public static GhostAnimator Instance { get; private set; }

    private Animator animator;

    public void Talk()
    {
        //play the talk animation
    }

    public void StopTalking()
    {
        //get back to idle
    }

    private void Awake()
    {
        SetInstance();
        animator = GetComponent<Animator>();
    }

    private void SetInstance()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
}

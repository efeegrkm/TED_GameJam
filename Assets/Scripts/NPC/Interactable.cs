using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))] 
public class Interactable : MonoBehaviour
{
    public bool isAutoTrigger = false; 
    public bool destroyAfterInteraction = false;
    public bool animateOnInteract = false; 
    public Animator animatorIfActive;
    public float invokeDelay = 0f; 

    [Header("Tetiklenecek Olaylar (Inspector'dan atayın)")]
    public UnityEvent OnInteractEvent;

    public void Interact()
    {
        if (animateOnInteract && animatorIfActive != null)
        {
            animatorIfActive.SetTrigger("Open");
        }

        if (invokeDelay > 0f)
        {
            PlayerMovementManager.Instance.StopMovement();
            Invoke("InvokeInteractEvent", invokeDelay);
        }
        else
        {
            OnInteractEvent?.Invoke();
        }

        if (destroyAfterInteraction)
        {
            this.gameObject.GetComponent<Collider>().enabled = false; 
            Destroy(this.gameObject, invokeDelay+0.1f);
        }
    }
    private void InvokeInteractEvent()
    {
        OnInteractEvent?.Invoke();
    }
}
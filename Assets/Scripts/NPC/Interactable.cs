using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))] 
public class Interactable : MonoBehaviour
{
    public bool isAutoTrigger = false; // Trigger on collider or wait for E

    [Header("Tetiklenecek Olaylar (Inspector'dan atayýn)")]
    public UnityEvent OnInteractEvent;

    public void Interact()
    {
        OnInteractEvent?.Invoke();
    }
}
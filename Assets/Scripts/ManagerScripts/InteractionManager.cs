using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    private Interactable currentInteractable;

    [SerializeField] private GameObject pressEGuide;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactableObj = other.GetComponent<Interactable>();
        if (interactableObj != null)
        {
            currentInteractable = interactableObj;

            if (interactableObj.isAutoTrigger)
            {
                TryInteract();
            }
            else
            {
                pressEGuide.SetActive(true);
                pressEGuide.GetComponent<TextMeshProUGUI>().text = "Press E to interact";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        pressEGuide.SetActive(false);

        Interactable interactableObj = other.GetComponent<Interactable>();
        if (interactableObj != null && currentInteractable == interactableObj)
        {
            currentInteractable = null; 
        }
    }

    public void TryInteract()
    {
        if (currentInteractable != null && GameManager.Instance.CurrentState == GameState.Exploring)
        {
            currentInteractable.Interact();
            pressEGuide.SetActive(false);
        }
    }
}
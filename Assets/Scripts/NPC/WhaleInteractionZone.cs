using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class WhaleInteractionZone : MonoBehaviour
{
    [Header("Etkileþim Ayarlarý")]
    [Tooltip("True ise balina çarpar çarpmaz tetiklenir. False ise yanýna gidip E'ye basmak gerekir.")]
    public bool isAutoTrigger = true;

    [Tooltip("Sýnýr bölgesi ise balinayý içeri sokmamak için geri itsin mi?")]
    public bool applyPushback = true;
    public float pushbackForce = 150f;

    [Header("Tetiklenecek Olaylar (Inspector'dan Atayýn)")]
    public UnityEvent OnInteractEvent;

    private bool isPlayerInZone = false;
    private Rigidbody2D playerRb;

    private void Update()
    {
        // Eðer E ile etkileþim modundaysak, oyuncu bölgedeyse ve oyun Balina modundaysa E tuþunu dinle
        if (!isAutoTrigger && isPlayerInZone && GameManager.Instance.CurrentState == GameState.OnWhale)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                ExecuteInteraction();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Çarpan þey balina mý ve balina modunda mýyýz?
        if (other.CompareTag("Player") && GameManager.Instance.CurrentState == GameState.OnWhale)
        {
            isPlayerInZone = true;
            playerRb = other.GetComponent<Rigidbody2D>();

            // Eðer otomatik tetiklenme açýksa beklemeden çalýþtýr
            if (isAutoTrigger)
            {
                ExecuteInteraction();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerRb = null;
        }
    }

    private void ExecuteInteraction()
    {
        // Geri itme açýksa balinayý geldiði yöne doðru it
        if (applyPushback && playerRb != null)
        {
            Vector2 pushDir = (playerRb.transform.position - transform.position).normalized;
            playerRb.AddForce(pushDir * pushbackForce, ForceMode2D.Impulse);
        }

        // Inspector'dan baðlanan tüm olaylarý (Diyalog, sahne geçiþi, ses çalma vs.) tetikle
        OnInteractEvent?.Invoke();
    }
}
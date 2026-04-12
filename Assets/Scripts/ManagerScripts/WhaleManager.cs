using UnityEngine;
using UnityEngine.InputSystem;

public class WhaleManager : MonoBehaviour
{
    public static WhaleManager Instance { get; private set; }

    [Header("UI Referanslarý")]
    public RectTransform parentCanvasRect; // Ana Canvas
    public RectTransform mapRect;          // Harita Image'ý
    public RectTransform whaleRect;        // Balina Image'ý
    public Rigidbody2D whaleRb;            // Balinanýn Fiziði

    [Header("Ayarlar")]
    public float moveSpeed = 400f;         // Balinanýn UI üzerindeki hýzý

    private Vector3 originalMapScale;
    private Vector2 originalMapPosition;
    private bool isWhaleMode = false;
    private Vector2 moveInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterWhaleMode()
    {
        // 1. Eski durumu kaydet
        mapRect.gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Map);
        //Blackout ekle zaman kalýrsa.
        originalMapScale = mapRect.localScale;
        originalMapPosition = mapRect.anchoredPosition;

        // 2. Haritayý ekranýn geniþliðine göre Scale et (Fit to Width)
        float targetScale = parentCanvasRect.rect.width / mapRect.rect.width;
        mapRect.localScale = new Vector3(targetScale, targetScale, 1f);

        // 3. Haritayý merkeze al
        mapRect.anchoredPosition = Vector2.zero;

        // 4. Sistemi ve fiziði aç
        isWhaleMode = true;
        whaleRb.simulated = true;
        GameManager.Instance.ChangeState(GameState.OnWhale);
    }

    // Bu metodu balina modundan çýkarken çaðýr
    public void ExitWhaleMode()
    {
        isWhaleMode = false;

        // Fiziði ve hýzý durdur
        whaleRb.simulated = false;
        whaleRb.linearVelocity = Vector2.zero; // (Unity 6 kullandýðýn için linearVelocity, eskiyse velocity yaz)

        // Haritayý eski konum ve boyutuna geri getir
        mapRect.localScale = originalMapScale;
        mapRect.anchoredPosition = originalMapPosition;

        GameManager.Instance.ChangeState(GameState.Map);
    }

    private void Update()
    {
        if (!isWhaleMode) return;

        // Diyalog açýlýrsa (veya Pause edilirse) balina olduðu yerde dursun
        if (GameManager.Instance.CurrentState != GameState.OnWhale)
        {
            if (whaleRb.linearVelocity != Vector2.zero) whaleRb.linearVelocity = Vector2.zero;
            return;
        }

        GetInput();

        HandleWhaleRotation();

        FollowWhaleVertical();
    }

    private void FixedUpdate()
    {
        if (!isWhaleMode || GameManager.Instance.CurrentState != GameState.OnWhale) return;

        // Balinayý Rigidbody ile hareket ettir (Duvarlara çarpabilmesi için bu þarttýr)
        // Harita büyüdüðü için hýzý mapRect.localScale.x ile çarpýyoruz ki hýz tutarlý kalsýn
        whaleRb.linearVelocity = moveInput * moveSpeed * mapRect.localScale.x;
    }

    private void GetInput()
    {
        moveInput = Vector2.zero;
        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;

        moveInput = moveInput.normalized;
    }

    private void HandleWhaleRotation()
    { 
        if (moveInput.x < 0)
        {
            whaleRect.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput.x > 0)
        {
            whaleRect.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void FollowWhaleVertical()
    {
        float whaleLocalY = whaleRect.localPosition.y;
        float targetMapY = -whaleLocalY * mapRect.localScale.y;

        float scaledMapHeight = mapRect.rect.height * mapRect.localScale.y;
        float canvasHeight = parentCanvasRect.rect.height;

        // Taþma payýný hesapla
        float maxY = Mathf.Max(0, (scaledMapHeight - canvasHeight) / 2f);
        float minY = -maxY;

        targetMapY = Mathf.Clamp(targetMapY, minY, maxY);

        // Haritayý yumuþak bir þekilde kaydýr (X ekseninde hep tam ortada kalýr)
        Vector2 currentPos = mapRect.anchoredPosition;
        currentPos.y = Mathf.Lerp(currentPos.y, targetMapY, Time.deltaTime * 10f);
        currentPos.x = 0;

        mapRect.anchoredPosition = currentPos;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhaleManager : MonoBehaviour
{
    public static WhaleManager Instance { get; private set; }

    [Header("UI Referansları")]
    public RectTransform parentCanvasRect;
    public RectTransform mapRect;
    public RectTransform whaleRect;
    public Rigidbody2D whaleRb;

    [Header("Ayarlar")]
    public float moveSpeed = 400f;

    private Vector3 originalMapScale;
    private Vector2 originalMapPosition;
    private bool isWhaleMode = false;
    private Vector2 moveInput;

    private bool canMove = true;

    [Header("Balina Konumları")]
    public List<Transform> whaleLocations;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void EnterWhaleMode()
    {
        mapRect.gameObject.SetActive(true);
        GameManager.Instance.ChangeState(GameState.Map);
        originalMapScale = mapRect.localScale;
        originalMapPosition = mapRect.anchoredPosition;

        float targetScale = parentCanvasRect.rect.width / mapRect.rect.width;
        mapRect.localScale = new Vector3(targetScale, targetScale, 1f);

        mapRect.anchoredPosition = Vector2.zero;

        isWhaleMode = true;
        whaleRb.simulated = true;
        GameManager.Instance.ChangeState(GameState.OnWhale);
    }

    public void ExitWhaleMode()
    {
        isWhaleMode = false;

        whaleRb.simulated = false;
        whaleRb.linearVelocity = Vector2.zero;

        mapRect.localScale = originalMapScale;
        mapRect.anchoredPosition = originalMapPosition;

        GameManager.Instance.ChangeState(GameState.Map);
    }

    private void Update()
    {
        if (!isWhaleMode) return;

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

        whaleRb.linearVelocity = moveInput * moveSpeed * mapRect.localScale.x;
    }

    public void StopWhaleMovement()
    {
        canMove = false;
    }

    public void ResumeWhaleMovement()
    {
        canMove = true;
    }

    private void GetInput()
    {
        moveInput = Vector2.zero;

        if (!canMove) return;

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

        float maxY = Mathf.Max(0, (scaledMapHeight - canvasHeight) / 2f);
        float minY = -maxY;

        targetMapY = Mathf.Clamp(targetMapY, minY, maxY);

        Vector2 currentPos = mapRect.anchoredPosition;
        currentPos.y = Mathf.Lerp(currentPos.y, targetMapY, Time.deltaTime * 10f);
        currentPos.x = 0;

        mapRect.anchoredPosition = currentPos;
    }

    public void FluctuateWhale(float intensity, float duration = 1f)
    {
        StartCoroutine(FluctuateRoutine(intensity, duration));
    }

    private System.Collections.IEnumerator FluctuateRoutine(float intensity, float duration)
    {
        Vector2 originalPos = whaleRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float randomX = UnityEngine.Random.Range(-1f, 1f) * intensity;
            float randomY = UnityEngine.Random.Range(-1f, 1f) * intensity;

            whaleRect.anchoredPosition = originalPos + new Vector2(randomX, randomY);

            elapsed += Time.deltaTime;
            yield return null;
        }

        whaleRect.anchoredPosition = originalPos;
    }
}
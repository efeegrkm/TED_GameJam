using System.Collections; 
using System; 
using UnityEngine;
using UnityEngine.UI;

public class GridBlock : MonoBehaviour
{
    [Header("Baþlangýç Koordinatý")]
    public int currentX;
    public int currentY;

    [Header("Olmasý Gereken (Doðru) Koordinat")]
    public int targetX;
    public int targetY;

    private GridPuzzle puzzleManager;
    private RectTransform rectTransform;
    private Image shadeImage;
    private float originalAlpha;

    public void Init(GridPuzzle manager, float width, float height)
    {
        puzzleManager = manager;
        rectTransform = GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(width, height);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        if (transform.childCount > 0)
        {
            shadeImage = transform.GetChild(0).GetComponent<Image>();
            if (shadeImage != null)
            {
                originalAlpha = shadeImage.color.a;
                shadeImage.gameObject.SetActive(false);
            }
        }
    }

    public void SetShade(bool isActive, bool isFirstSelection = true)
    {
        if (shadeImage == null) return;

        shadeImage.gameObject.SetActive(isActive);

        if (isActive)
        {
            Color targetColor = Color.black;
            targetColor.a = originalAlpha;
            shadeImage.color = targetColor;
        }
    }

    public void UpdateVisualPosition(bool instant = true, Action onComplete = null)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        float xPos = (currentX * rectTransform.sizeDelta.x) + (rectTransform.sizeDelta.x / 2f);
        float yPos = (currentY * rectTransform.sizeDelta.y) + (rectTransform.sizeDelta.y / 2f);
        Vector2 targetPos = new Vector2(xPos, yPos);

        if (instant)
        {
            rectTransform.anchoredPosition = targetPos;
            onComplete?.Invoke();
        }
        else
        {
            StartCoroutine(AnimateMove(targetPos, puzzleManager.swapDuration, onComplete));
        }
    }

    private IEnumerator AnimateMove(Vector2 targetPos, float duration, Action onComplete)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        rectTransform.anchoredPosition = targetPos;
        onComplete?.Invoke();
    }
}
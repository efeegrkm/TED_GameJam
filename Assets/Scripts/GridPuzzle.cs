using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GridPuzzle : MonoBehaviour
{
    [Header("Puzzle Boyutlarý")]
    public int xGridCount = 3;
    public int yGridCount = 3;

    [Header("Harita Parçalarý")]
    public List<GridBlock> blocks;

    [Header("Görsel Ayarlar")]
    public Color gridLineColor = new Color(0, 0, 0, 0.5f);
    public float gridLineThickness = 4f;

    // --- YENÝ EKLENEN GÖRSEL AYARLAR ---
    [Header("Efektler ve Ok")]
    public float swapDuration = 0.2f; // Yer deðiþtirme süresi
    public RectTransform arrowIndicator; // Hazýrladýðýmýz Ok objesi
    // -----------------------------------

    private RectTransform puzzleRect;
    private float blockWidth;
    private float blockHeight;
    private GameObject linesContainer;

    private bool isPuzzleActive = false;
    private bool isAnimating = false; // Bloklar kayarken inputu kitlemek için
    private int selectionState = 0;
    private int cursorX = 0;
    private int cursorY = 0;
    private GridBlock firstSelectedBlock;
    private GridBlock secondSelectedBlock;

    void OnEnable()
    {
        InitializePuzzle();
    }

    private void InitializePuzzle()
    {
        int expectedBlockCount = xGridCount * yGridCount;
        if (blocks.Count != expectedBlockCount) return;

        puzzleRect = GetComponent<RectTransform>();
        blockWidth = puzzleRect.rect.width / xGridCount;
        blockHeight = puzzleRect.rect.height / yGridCount;

        foreach (var block in blocks)
        {
            block.Init(this, blockWidth, blockHeight);
            block.UpdateVisualPosition(true); // Baþlangýçta anýnda(instant) dizil
        }

        CreateGridLines();
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);

        cursorX = 0;
        cursorY = yGridCount - 1;
        selectionState = 0;
        isPuzzleActive = true;
        isAnimating = false;
        firstSelectedBlock = null;
        secondSelectedBlock = null;
        UpdateCursorVisuals();
    }

    void Update()
    {
        if (!isPuzzleActive || isAnimating) return;

        if (selectionState == 0) HandleFirstSelectionInput();
        else if (selectionState == 1) HandleSecondSelectionInput();
    }

    private void HandleFirstSelectionInput()
    {
        if (Keyboard.current == null) return;
        int dx = 0, dy = 0;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) dy = 1;
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) dy = -1;
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame) dx = 1;
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) dx = -1;

        if (dx != 0 || dy != 0)
        {
            int newX = cursorX + dx;
            int newY = cursorY + dy;

            if (newX >= 0 && newX < xGridCount && newY >= 0 && newY < yGridCount)
            {
                cursorX = newX;
                cursorY = newY;
                UpdateCursorVisuals();
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            firstSelectedBlock = GetBlockAt(cursorX, cursorY);
            selectionState = 1;

            secondSelectedBlock = GetDefaultAdjacentBlock(firstSelectedBlock);

            UpdateCursorVisuals();
        }
    }

    private void HandleSecondSelectionInput()
    {
        if (Keyboard.current == null) return;
        int dx = 0, dy = 0;
        bool inputReceived = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) { dx = 0; dy = 1; inputReceived = true; }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame) { dx = 0; dy = -1; inputReceived = true; }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame) { dx = 1; dy = 0; inputReceived = true; }
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) { dx = -1; dy = 0; inputReceived = true; }

        if (inputReceived)
        {
            int targetX = firstSelectedBlock.currentX + dx;
            int targetY = firstSelectedBlock.currentY + dy;

            if (targetX >= 0 && targetX < xGridCount && targetY >= 0 && targetY < yGridCount)
            {
                secondSelectedBlock = GetBlockAt(targetX, targetY);
                UpdateCursorVisuals();
            }
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (secondSelectedBlock != null)
            {
                SwapBlocks(firstSelectedBlock, secondSelectedBlock);
                if (!isPuzzleActive) return;

                cursorX = firstSelectedBlock.currentX;
                cursorY = firstSelectedBlock.currentY;

                selectionState = 0;
                firstSelectedBlock = null;
                secondSelectedBlock = null;
                UpdateCursorVisuals();
            }
            else
            {
                selectionState = 0;
                firstSelectedBlock = null;
                UpdateCursorVisuals();
            }
        }
    }

    private void UpdateCursorVisuals()
    {
        foreach (var block in blocks) block.SetShade(false);
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false); // Oku gizle

        if (selectionState == 0)
        {
            GridBlock cb = GetBlockAt(cursorX, cursorY);
            if (cb != null) cb.SetShade(true, true);
        }
        else if (selectionState == 1)
        {
            if (firstSelectedBlock != null) firstSelectedBlock.SetShade(true, true);

            if (secondSelectedBlock != null)
            {
                secondSelectedBlock.SetShade(true, false);

                if (arrowIndicator != null)
                {
                    arrowIndicator.gameObject.SetActive(true);

                    arrowIndicator.SetAsLastSibling();

                    Vector3 pos1 = firstSelectedBlock.GetComponent<RectTransform>().position;
                    Vector3 pos2 = secondSelectedBlock.GetComponent<RectTransform>().position;

                    arrowIndicator.position = (pos1 + pos2) / 2f;

                    int diffX = secondSelectedBlock.currentX - firstSelectedBlock.currentX;
                    int diffY = secondSelectedBlock.currentY - firstSelectedBlock.currentY;

                    if (diffX == 1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 0);       // Sað
                    else if (diffX == -1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 180); // Sol
                    else if (diffY == 1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 90);  // Yukarý
                    else if (diffY == -1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, -90); // Aþaðý
                }
            }
        }
    }

    // Çizgi oluþturma metodlarýnda deðiþiklik yok...
    private void CreateGridLines()
    {
        if (linesContainer != null) Destroy(linesContainer);
        linesContainer = new GameObject("GridLines");
        linesContainer.transform.SetParent(puzzleRect, false);

        RectTransform containerRt = linesContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = Vector2.zero; containerRt.anchorMax = Vector2.one;
        containerRt.offsetMin = Vector2.zero; containerRt.offsetMax = Vector2.zero;

        for (int i = 1; i < xGridCount; i++) CreateLine(new Vector2(i * blockWidth, puzzleRect.rect.height / 2f), new Vector2(gridLineThickness, puzzleRect.rect.height), "V_Line_" + i);
        for (int i = 1; i < yGridCount; i++) CreateLine(new Vector2(puzzleRect.rect.width / 2f, i * blockHeight), new Vector2(puzzleRect.rect.width, gridLineThickness), "H_Line_" + i);

        CreateLine(new Vector2(0, puzzleRect.rect.height / 2f), new Vector2(gridLineThickness, puzzleRect.rect.height), "Border_Left");
        CreateLine(new Vector2(puzzleRect.rect.width, puzzleRect.rect.height / 2f), new Vector2(gridLineThickness, puzzleRect.rect.height), "Border_Right");
        CreateLine(new Vector2(puzzleRect.rect.width / 2f, 0), new Vector2(puzzleRect.rect.width, gridLineThickness), "Border_Bottom");
        CreateLine(new Vector2(puzzleRect.rect.width / 2f, puzzleRect.rect.height), new Vector2(puzzleRect.rect.width, gridLineThickness), "Border_Top");

        linesContainer.transform.SetAsLastSibling();
    }

    private void CreateLine(Vector2 pos, Vector2 size, string lineName)
    {
        GameObject lineObj = new GameObject(lineName);
        lineObj.transform.SetParent(linesContainer.transform, false);
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = gridLineColor;
        lineImg.raycastTarget = false;
        RectTransform rt = lineObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f); rt.sizeDelta = size; rt.anchoredPosition = pos;
    }

    // --- DEÐÝÞEN KISIM: Animasyonu tetikleme ---
    public void SwapBlocks(GridBlock blockA, GridBlock blockB)
    {
        int tempX = blockA.currentX;
        int tempY = blockA.currentY;

        blockA.currentX = blockB.currentX;
        blockA.currentY = blockB.currentY;

        blockB.currentX = tempX;
        blockB.currentY = tempY;

        isAnimating = true; // Inputlarý kitle
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false); // Kayarken oku sakla

        int completedAnimations = 0;

        // Bu Action, her iki bloðun da animasyonu bittiðinde çaðrýlacak
        System.Action onAnimComplete = () => {
            completedAnimations++;
            if (completedAnimations == 2) // Ýki blok da hedefine vardýysa
            {
                isAnimating = false; // Kilit aç
                CheckWinCondition(); // Kazanma kontrolünü YAP!
            }
        };

        // Ýki bloða da "Animasyonlu git ve bitince haber ver" diyoruz
        blockA.UpdateVisualPosition(false, onAnimComplete);
        blockB.UpdateVisualPosition(false, onAnimComplete);
    }

    public GridBlock GetBlockAt(int x, int y)
    {
        foreach (var block in blocks)
        {
            if (block.currentX == x && block.currentY == y) return block;
        }
        return null;
    }

    private void CheckWinCondition()
    {
        foreach (var block in blocks)
        {
            if (block.currentX != block.targetX || block.currentY != block.targetY) return;
        }
        FinishedPuzzleSuccessfully();
    }

    private GridBlock GetDefaultAdjacentBlock(GridBlock baseBlock)
    {
        if (baseBlock == null) return null;

        // Harita sýnýrlarýndan dýþarý çýkmamak için sýrasýyla Sað, Sol, Yukarý ve Aþaðýyý kontrol et
        if (baseBlock.currentX + 1 < xGridCount) return GetBlockAt(baseBlock.currentX + 1, baseBlock.currentY);
        if (baseBlock.currentX - 1 >= 0) return GetBlockAt(baseBlock.currentX - 1, baseBlock.currentY);
        if (baseBlock.currentY + 1 < yGridCount) return GetBlockAt(baseBlock.currentX, baseBlock.currentY + 1);
        if (baseBlock.currentY - 1 >= 0) return GetBlockAt(baseBlock.currentX, baseBlock.currentY - 1);

        return null;
    }

    private void FinishedPuzzleSuccessfully()
    {
        isPuzzleActive = false;
        foreach (var block in blocks) block.SetShade(false);
        if (linesContainer != null) linesContainer.SetActive(false);
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);
    }
}
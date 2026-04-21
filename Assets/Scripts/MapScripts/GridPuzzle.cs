using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class GridPuzzle : MonoBehaviour
{
    [Header("Puzzle Boyutları")]
    public int xGridCount = 3;
    public int yGridCount = 3;

    [Header("Harita Parçaları")]
    public List<GridBlock> blocks;

    [Header("Görsel Ayarlar")]
    [Tooltip("Bitmiş map resize için:")]
    public Sprite finishedPuzzle; 

    public Color gridLineColor = new Color(0, 0, 0, 0.5f);
    public float gridLineThickness = 4f;

    [Header("Efektler ve Ok")]
    public float swapDuration = 0.2f;
    public RectTransform arrowIndicator;

    private RectTransform puzzleRect;
    private float blockWidth;
    private float blockHeight;
    private GameObject linesContainer;
    public Animator animatorMan;

    private bool isPuzzleActive = false;
    private bool isAnimating = false;
    private int selectionState = 0;
    private int cursorX = 0;
    private int cursorY = 0;
    private GridBlock firstSelectedBlock;
    private GridBlock secondSelectedBlock;

    [Header("Map Yamaları")]
    [SerializeField] private GameObject mapYama1;
    [SerializeField] private GameObject mapYama2;
    [SerializeField] private GameObject mapYama3;
    [SerializeField] private GameObject mapYama4;
    [SerializeField] private GameObject mapYama5;

    public bool devShortCut = false;
    void OnEnable()
    {
        InitializePuzzle();
    }

    private void InitializePuzzle()
    {
        GameManager.Instance.ChangeState(GameState.Puzzle);
        int expectedBlockCount = xGridCount * yGridCount;
        if (blocks.Count != expectedBlockCount) return;

        puzzleRect = GetComponent<RectTransform>();
        if (finishedPuzzle != null)
        {
            float imageWidth = finishedPuzzle.rect.width;
            float imageHeight = finishedPuzzle.rect.height;
            float aspectRatio = imageWidth / imageHeight;

            float currentWidth = puzzleRect.rect.width;
            float newHeight = currentWidth / aspectRatio;

            puzzleRect.sizeDelta = new Vector2(currentWidth, newHeight);
        }

        blockWidth = puzzleRect.rect.width / xGridCount;
        blockHeight = puzzleRect.rect.height / yGridCount;

        foreach (var block in blocks)
        {
            block.Init(this, blockWidth, blockHeight);
            block.UpdateVisualPosition(true);
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
        if(devShortCut)
        {
            FinishedPuzzleSuccessfully();
            devShortCut = false;
        }
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
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);

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

                    if (diffX == 1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 0);       // Sa�
                    else if (diffX == -1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 180); // Sol
                    else if (diffY == 1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, 90);  // Yukar�
                    else if (diffY == -1) arrowIndicator.localRotation = Quaternion.Euler(0, 0, -90); // A�a��
                }
            }
        }
    }

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

    public void SwapBlocks(GridBlock blockA, GridBlock blockB)
    {
        int tempX = blockA.currentX;
        int tempY = blockA.currentY;

        blockA.currentX = blockB.currentX;
        blockA.currentY = blockB.currentY;

        blockB.currentX = tempX;
        blockB.currentY = tempY;

        isAnimating = true;
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);

        int completedAnimations = 0;

        System.Action onAnimComplete = () => {
            completedAnimations++;
            if (completedAnimations == 2)
            {
                isAnimating = false;
                CheckWinCondition();
            }
        };

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

        if (baseBlock.currentX + 1 < xGridCount) return GetBlockAt(baseBlock.currentX + 1, baseBlock.currentY);
        if (baseBlock.currentX - 1 >= 0) return GetBlockAt(baseBlock.currentX - 1, baseBlock.currentY);
        if (baseBlock.currentY + 1 < yGridCount) return GetBlockAt(baseBlock.currentX, baseBlock.currentY + 1);
        if (baseBlock.currentY - 1 >= 0) return GetBlockAt(baseBlock.currentX, baseBlock.currentY - 1);

        return null;
    }

    private void FinishedPuzzleSuccessfully()
    {
        string puzzleIndex = this.gameObject.tag;
        GameManager.Instance.ChangeState(GameState.Exploring);
        initiateCurrentPuzzleFinish(puzzleIndex);
        isPuzzleActive = false;
        foreach (var block in blocks) block.SetShade(false);
        if (linesContainer != null) linesContainer.SetActive(false);
        if (arrowIndicator != null) arrowIndicator.gameObject.SetActive(false);
    }
    private void initiateCurrentPuzzleFinish(string puzzleIndex)
    {
        switch (puzzleIndex)
        {
            case "puzzle 1":
                PlayerMovementManager.Instance.StopMovement();
                mapYama1.SetActive(true);
                EventManager.Instance.mapReceived[0] = true;
                List<DialogueLine> conversation = new List<DialogueLine>
                {
                    new DialogueLine("Hayalet", "Bu kadar akıllı olduğunu bilmiyordum aşkitom."),
                    new DialogueLine("Hayalet", "Sonraki 4 bulmaca bu kadar kolay olsa keşke..."),
                    new DialogueLine("Hayalet", "Sana birleştirdiğin harita parçalarını üzerinde toplaman için tüm haritanın bir taslağını veriyorum bu sana yol gösterecek."),
                    new DialogueLine("Hayalet", "'M' ile üzerinde harita parçalarını birleştirdiğin harita taslağını açabilirsin."),
                    new DialogueLine("Hayalet", "Her bulduğun yeni haritada bi sonraki adanın haritasının olduğu sandıkların yerleri işaretli olacak."),
                    new DialogueLine("Hayalet", "En azından öyle umuyorum..."),
                    new DialogueLine("Prenses", "Hmm...")
                };
                DialogueManager.Instance.StartDialogue(conversation, () =>
                {
                    this.gameObject.SetActive(false);
                    PlayerMovementManager.Instance.ResumeMovement();
                });
                break;
            case "puzzle 2":
                PlayerMovementManager.Instance.StopMovement();
                mapYama2.SetActive(true);
                EventManager.Instance.mapReceived[1] = true;
                List<DialogueLine> conversation1 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "2. harita da tamam."),
                    new DialogueLine("Hayalet", "Her geçen an ümidim artıyor. Haritaya 2. parçayı da ekle."),
                    new DialogueLine("Hayalet", "Ha bu arada... Otiyle aranızdaki ufak meseleyi çozdum."),
                    new DialogueLine("Hayalet", "Oti adalar arası seyahat edebilmen için tek çaremiz..."),
                    new DialogueLine("Prenses", "Ay tamam be. Alttan alirim biraz baligini."),
                    new DialogueLine("Hayalet", "..."),
                    new DialogueLine("Hayalet", "Oti seni bekliyor... Haritayı tamamladığına göre diğer adaya geçmeye hazırsın.")
                };
                DialogueManager.Instance.StartDialogue(conversation1, () =>
                {
                    this.gameObject.SetActive(false);
                    PlayerMovementManager.Instance.ResumeMovement();
                });
                break;
            case "puzzle 3":
                PlayerMovementManager.Instance.StopMovement();
                mapYama3.SetActive(true);
                EventManager.Instance.mapReceived[2] = true;
                EventManager.Instance.mapAssured = true;
                List<DialogueLine> conversation4 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "3.map de halloldu.")
                };
                DialogueManager.Instance.StartDialogue(conversation4, () =>
                {
                    this.gameObject.SetActive(false);
                    PlayerMovementManager.Instance.ResumeMovement();
                });
                
                break;
            case "puzzle 4":
                PlayerMovementManager.Instance.StopMovement();
                mapYama4.SetActive(true);
                EventManager.Instance.mapReceived[3] = true;
                EventManager.Instance.mapAssured = true;
                List<DialogueLine> conversation5 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "4.map de halloldu.")
                };
                DialogueManager.Instance.StartDialogue(conversation5, () =>
                {
                    this.gameObject.SetActive(false);
                    PlayerMovementManager.Instance.ResumeMovement();
                });
                break;
            case "puzzle 5":
                PlayerMovementManager.Instance.StopMovement();
                mapYama5.SetActive(true);
                EventManager.Instance.mapReceived[4] = true;
                EventManager.Instance.mapAssured = true;
                List<DialogueLine> conversation6 = new List<DialogueLine>
                {
                    new DialogueLine("Prenses", "Sonunda..."),
                    new DialogueLine("Prenses", "Bu neydi kız!! Neyse ki çok zeki bir prensesim..."),
                    new DialogueLine("Prenses", "Haritanın tamamını birleştirdim. Şimdi sadece Otinin ağzına son bir kez girmek kaldı."),
                };
                DialogueManager.Instance.StartDialogue(conversation6, () =>
                {
                    EventManager.Instance.mapFinishEventTrigger = true;
                    this.gameObject.SetActive(false);
                    PlayerMovementManager.Instance.ResumeMovement();    
                });
                break;
            case "puzzle 6":
                EventManager.Instance.initiateFinalPuzzleFinish(this.gameObject);
                break;
            default:
                Debug.LogWarning("Bilinmeyen puzzle index'i: " + puzzleIndex);
                break;
        }
    }
}
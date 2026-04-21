using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System;

[System.Serializable]
public class Speaker
{
    public string speakerName;
    public Sprite portraitIcon;
}

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(3, 10)]
    public string text;

    public DialogueLine(string name, string txt)
    {
        speakerName = name;
        text = txt;
    }
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Veritabaný")]
    [Tooltip("Tüm konuþmacýlarý (Ýsim ve Ýkon) buraya ekleyin")]
    public List<Speaker> speakers;

    [Header("UI Referanslarý")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Ayarlar")]
    public float typingSpeed = 0.03f;

    private Queue<DialogueLine> linesQueue = new Queue<DialogueLine>();
    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;

    private Action onDialogueCompleteCallback;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!dialoguePanel.activeInHierarchy) return;

        bool skipOrNextPressed = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) skipOrNextPressed = true;
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)) skipOrNextPressed = true;

        if (skipOrNextPressed)
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentFullText;
                isTyping = false;
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    public void ShowSingleLine(string speakerName, string text)
    {
        if (dialoguePanel.activeInHierarchy) return;

        linesQueue.Clear();
        linesQueue.Enqueue(new DialogueLine(speakerName, text));
        StartDialogueSequence();
    }

    public void StartDialogue(List<DialogueLine> dialogueLines, Action onComplete = null)
    {
        if (dialoguePanel.activeInHierarchy) return;

        GameManager.Instance.ChangeState(GameState.Dialogue);

        onDialogueCompleteCallback = onComplete;

        linesQueue.Clear();
        foreach (DialogueLine line in dialogueLines)
        {
            linesQueue.Enqueue(line);
        }
        StartDialogueSequence();
    }

    private void StartDialogueSequence()
    {
        dialoguePanel.SetActive(true);
        DisplayNextLine();
    }

    private void DisplayNextLine()
    {
        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = linesQueue.Dequeue();
        SetupSpeakerUI(line.speakerName);

        currentFullText = line.text;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentFullText));
    }

    private void SetupSpeakerUI(string currentSpeakerName)
    {
        Speaker currentSpeaker = speakers.Find(s => s.speakerName.ToLower() == currentSpeakerName.ToLower());

        if (currentSpeaker != null)
        {
            portraitImage.sprite = currentSpeaker.portraitIcon;
            nameText.text = currentSpeaker.speakerName;
        }
        else
        {
            Debug.LogWarning("Speaker bulunamadý: " + currentSpeakerName);
            portraitImage.sprite = null;
            nameText.text = currentSpeakerName;
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        int count = 0;
        foreach (char letter in sentence.ToCharArray())
        {
            if (count % 2 == 0)
            {
                SoundManager.Instance.PlayAudio();
            }
            dialogueText.text += letter;

            count++;

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        if (GameManager.Instance.CurrentState == GameState.Dialogue)
            GameManager.Instance.ChangeState(GameState.Exploring);

        dialoguePanel.SetActive(false);

        if (onDialogueCompleteCallback != null)
        {
            Action tempCallback = onDialogueCompleteCallback;
            onDialogueCompleteCallback = null;
            tempCallback.Invoke();
        }
    }
}
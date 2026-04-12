using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [SerializeField] private GameObject map;
    [SerializeField] private GameObject pauseScreen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ToggleMap()
    {
        if (GameManager.Instance.CurrentState == GameState.Exploring)
        {
            GameManager.Instance.ChangeState(GameState.Map);
            openMap();
        }
        else if (GameManager.Instance.CurrentState == GameState.Map)
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
            closeMap();
        }
    }

    public void TogglePause()
    {
        if (GameManager.Instance.CurrentState == GameState.Exploring)
        {
            GameManager.Instance.ChangeState(GameState.Paused);
            pause();
        }
        else if (GameManager.Instance.CurrentState == GameState.Paused)
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
            resume();
        }
    }

    public void openMap()
    {
        map.SetActive(true);
    }
    public void closeMap()
    {
        map.SetActive(false);   
    }
    public void pause()
    {
        pauseScreen.SetActive(true);
        Time.timeScale = 0f; 
    }
    public void resume()
    {
        Time.timeScale = 1f;
        pauseScreen.SetActive(false);
    }
    public void testDialogue1()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("TestSpeaker1", "Merhaba, haritadaki son koordinatlarý kontrol ettin mi?"),
            new DialogueLine("TestSpeaker2", "Evet, radar sistemini yeni güncelledim. Oraya gitmemiz biraz tehlikeli olabilir."),
            new DialogueLine("TestSpeaker3", "O zaman dikkatli olmalýyýz.")
        };

        DialogueManager.Instance.StartDialogue(conversation);
    }

    public void whaleOn1()
    {
        WhaleManager.Instance.EnterWhaleMode();
    }

    public void restrictedKraken()
    {
        List<DialogueLine> conversation = new List<DialogueLine>
        {
            new DialogueLine("Whale", "Krakene tükürürürüm seni, yolunu deðiþtir."),
            new DialogueLine("Prenses", "...")
        };

        DialogueManager.Instance.StartDialogue(conversation);
        GameManager.Instance.ChangeState(GameState.OnWhale);
    }

    public void startIsland2()
    {

    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public GameObject title;
    public GameObject subtitle;
    public GameObject pressToStart;
    public GameObject blackout;

    public GameObject GameScreenRoot;

    public Animator CalicoAnim;
    private Animator blackoutAnimator;
    private Animator settingsAnimator;
    private Animator titleAnimator;
    private Animator subtitleAnimator;

    private int currentMenuStage = 0;

    private void Awake()
    {
        settingsAnimator = SettingsMenu.GetComponent<Animator>();
        titleAnimator = title.GetComponent<Animator>();
        subtitleAnimator = subtitle.GetComponent<Animator>();
        blackoutAnimator = blackout.GetComponent<Animator>();

        blackout.SetActive(true);
        MainMenu.SetActive(true);
        subtitle.SetActive(true);
        CalicoAnim.gameObject.SetActive(false);
        pressToStart.SetActive(false);
        SettingsMenu.SetActive(false);
        GameScreenRoot.SetActive(false);
    }

    void Start()
    {
        StartCoroutine(MenuScreenRoutine());
    }

    private void Update()
    {
        if (!pressToStart.activeInHierarchy) return;

        bool anyKeyPressed = false;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) anyKeyPressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) anyKeyPressed = true;

        if (anyKeyPressed)
        {
            if (currentMenuStage == 0)
            {
                currentMenuStage++;
                StartCoroutine(SettingsScreenRoutine());
            }
            else if (currentMenuStage == 1)
            {
                currentMenuStage++;
                StartCoroutine(StartGameScreenRoutine());
            }
        }
    }

    private IEnumerator MenuScreenRoutine()
    {
        yield return new WaitForSeconds(1);
        blackoutAnimator.SetTrigger("light");
        yield return new WaitForSeconds(2);
        subtitleAnimator.SetTrigger("appear");
        yield return new WaitForSeconds(1);
        pressToStart.SetActive(true);
        yield break;
    }

    private IEnumerator SettingsScreenRoutine()
    {
        pressToStart.SetActive(false);
        subtitleAnimator.SetTrigger("dissapear");
        yield return new WaitForSeconds(0.5f);
        titleAnimator.SetTrigger("dissapear");
        yield return new WaitForSeconds(0.5f);
        SettingsMenu.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        CalicoAnim.gameObject.SetActive(true);
        pressToStart.SetActive(true);
    }

    private IEnumerator StartGameScreenRoutine()
    {
        pressToStart.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        settingsAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(0.5f);
        blackoutAnimator.SetTrigger("blackout");
        yield return new WaitForSeconds(4f);
        CalicoAnim.SetTrigger("dissapear");
        yield return new WaitForSeconds(2f);
        MainMenu.SetActive(false);

        GameScreenRoot.SetActive(true);
        CalicoAnim.gameObject.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameMenuUI : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Button playButton;

    [Header("Start behavior")]
    [SerializeField] private bool activateOnStart = true;
    [SerializeField] private bool fadeOnStart = true;

    [Header("Play behavior")]
    [SerializeField] private bool fadeOnPlay = true;
    [SerializeField] private UnityEvent onPlay;

    private GameSettings gameSettings;
    private bool buttonHooked;

    private void Awake()
    {
        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        gameSettings = FindFirstObjectByType<GameSettings>();
        HookPlayButton();

        if (activateOnStart && fadeOnStart)
        {
            menuRoot.SetActive(false);
        }
    }

    private void Start()
    {
        if (!activateOnStart)
        {
            return;
        }

        if (fadeOnStart && gameSettings != null)
        {
            menuRoot.SetActive(false);
            gameSettings.FadeAndSwitch(() => menuRoot.SetActive(true));
            return;
        }

        menuRoot.SetActive(true);
    }

    private void HookPlayButton()
    {
        if (buttonHooked || playButton == null)
        {
            return;
        }

        playButton.onClick.AddListener(HandlePlayClicked);
        buttonHooked = true;
    }

    private void HandlePlayClicked()
    {
        SFXController.Instance?.PlayUIButtonClick();
        if (fadeOnPlay && gameSettings != null)
        {
            gameSettings.FadeAndSwitch(() =>
            {
                menuRoot.SetActive(false);
                onPlay?.Invoke();
            });
            return;
        }

        menuRoot.SetActive(false);
        onPlay?.Invoke();
    }
}

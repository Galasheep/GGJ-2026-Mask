using UnityEngine;
using UnityEngine.UI;

public class PrefabSwitchList : MonoBehaviour
{
    [System.Serializable]
    public struct ButtonTarget
    {
        public Button button;
        public GameObject target;
    }

    [Header("Buttons mapped to their target prefabs")]
    [SerializeField] private ButtonTarget[] buttons;

    [Header("Optional parent to deactivate on click")]
    [SerializeField] private GameObject parentToDeactivate;

    private GameSettings gameSettings;

    private void Awake()
    {
        gameSettings = FindFirstObjectByType<GameSettings>();

        if (buttons == null)
        {
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i].button;
            GameObject target = buttons[i].target;

            if (button == null)
            {
                continue;
            }

            button.onClick.AddListener(() => ActivateTarget(target));
        }
    }

    private void ActivateTarget(GameObject target)
    {
        if (gameSettings != null)
        {
            gameSettings.FadeAndSwitch(() => SwitchNow(target));
            return;
        }

        SwitchNow(target);
    }

    private void SwitchNow(GameObject target)
    {
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].target != null)
                {
                    buttons[i].target.SetActive(false);
                }
            }
        }

        if (target != null)
        {
            target.SetActive(true);
        }

        if (parentToDeactivate != null)
        {
            parentToDeactivate.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

public class PrefabSwitchList : MonoBehaviour
{
    [Header("UI asset list")]
    [SerializeField] private MaskUiAssetList uiAssets;

    [Header("Optional mask UI controller")]
    [SerializeField] private maskfeature maskFeature;

    [System.Serializable]
    public struct ButtonTarget
    {
        public Button button;
        public GameObject target;
        public MaskUiAssetList uiAssetsOverride;
    }

    [Header("Buttons mapped to their target prefabs")]
    [SerializeField] private ButtonTarget[] buttons;

    [Header("Riddle button sequence")]
    [SerializeField] private Button[] riddleButtons;
    [SerializeField] private int[] riddleOrder;
    [SerializeField] private GameObject riddleTarget;
    [SerializeField] private bool resetRiddleOnWrong = true;
    [SerializeField] private bool disableRiddleButtonsOnSuccess = true;

    [Header("Back button destination (optional override)")]
    [SerializeField] private GameObject backTargetOverride;

    [Header("Optional parent to deactivate on click")]
    [SerializeField] private GameObject parentToDeactivate;

    [Header("Hover alpha effect")]
    [SerializeField] private bool enableHoverAlpha = true;
    [SerializeField, Range(0f, 1f)] private float hoverOnAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float hoverOffAlpha = 0f;

    private GameSettings gameSettings;
    private maskfeature cachedMaskFeature;
    private int riddleProgress;
    private GameObject currentTarget;
    private readonly System.Collections.Generic.Stack<GameObject> backHistory =
        new System.Collections.Generic.Stack<GameObject>();

    private void OnEnable()
    {
        ResetRiddleProgress();
        CacheCurrentTargetFromScene();

        if (gameSettings != null)
        {
            gameSettings.SetActiveSwitchList(this);
            gameSettings.RefreshBackButton();
        }

        maskfeature feature = ResolveMaskFeature();
        if (feature != null)
        {
            feature.ApplyList(uiAssets);
        }
    }

    private void OnDisable()
    {
        if (gameSettings != null)
        {
            gameSettings.ClearActiveSwitchList(this);
            gameSettings.RefreshBackButton();
        }
    }

    public MaskUiAssetList GetUiAssets()
    {
        return uiAssets;
    }

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
            int buttonIndex = i;

            if (button == null)
            {
                continue;
            }

            if (enableHoverAlpha)
            {
                ButtonHoverAlpha hover = button.GetComponent<ButtonHoverAlpha>();
                if (hover == null)
                {
                    hover = button.gameObject.AddComponent<ButtonHoverAlpha>();
                }

                hover.Configure(hoverOnAlpha, hoverOffAlpha);
            }

            button.onClick.AddListener(() => ActivateTarget(target, buttonIndex));
        }

        if (riddleButtons != null)
        {
            for (int i = 0; i < riddleButtons.Length; i++)
            {
                Button button = riddleButtons[i];
                int buttonIndex = i;

                if (button == null)
                {
                    continue;
                }

                if (enableHoverAlpha)
                {
                    ButtonHoverAlpha hover = button.GetComponent<ButtonHoverAlpha>();
                    if (hover == null)
                    {
                        hover = button.gameObject.AddComponent<ButtonHoverAlpha>();
                    }

                    hover.Configure(hoverOnAlpha, hoverOffAlpha);
                }

                button.onClick.AddListener(() => HandleRiddlePress(buttonIndex));
            }
        }
    }

    private void ActivateTarget(GameObject target, int buttonIndex)
    {
        if (gameSettings != null)
        {
            gameSettings.FadeAndSwitch(() => SwitchNow(target, buttonIndex, true));
            return;
        }

        SwitchNow(target, buttonIndex, true);
    }

    private void HandleRiddlePress(int buttonIndex)
    {
        if (riddleButtons == null || riddleOrder == null || riddleOrder.Length == 0)
        {
            return;
        }

        if (riddleProgress < 0 || riddleProgress >= riddleOrder.Length)
        {
            ResetRiddleProgress();
        }

        int expectedIndex = riddleOrder[riddleProgress];
        if (buttonIndex == expectedIndex)
        {
            riddleProgress++;
            if (riddleProgress >= riddleOrder.Length)
            {
                ActivateRiddleTarget();
            }
        }
        else if (resetRiddleOnWrong)
        {
            ResetRiddleProgress();
        }
    }

    private void ActivateRiddleTarget()
    {
        if (gameSettings != null)
        {
            gameSettings.FadeAndSwitch(ActivateRiddleNow);
            return;
        }

        ActivateRiddleNow();
    }

    private void ActivateRiddleNow()
    {
        if (riddleTarget != null)
        {
            riddleTarget.SetActive(true);
        }

        if (disableRiddleButtonsOnSuccess && riddleButtons != null)
        {
            for (int i = 0; i < riddleButtons.Length; i++)
            {
                if (riddleButtons[i] != null)
                {
                    riddleButtons[i].interactable = false;
                }
            }
        }

        if (parentToDeactivate != null)
        {
            parentToDeactivate.SetActive(false);
        }
    }

    private void SwitchNow(GameObject target, int buttonIndex, bool recordHistory)
    {
        if (recordHistory && currentTarget != null && currentTarget != target)
        {
            backHistory.Push(currentTarget);
        }

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

        currentTarget = target;
        if (gameSettings != null)
        {
            gameSettings.RefreshBackButton();
        }

        maskfeature feature = ResolveMaskFeature();
        if (feature != null)
        {
            MaskUiAssetList listToUse = uiAssets;
            if (target != null)
            {
                PrefabSwitchList targetList = target.GetComponentInChildren<PrefabSwitchList>(true);
                if (targetList != null && targetList != this)
                {
                    MaskUiAssetList targetAssets = targetList.GetUiAssets();
                    if (targetAssets != null)
                    {
                        listToUse = targetAssets;
                    }
                }
            }

            if (buttons != null && buttonIndex >= 0 && buttonIndex < buttons.Length)
            {
                MaskUiAssetList overrideAssets = buttons[buttonIndex].uiAssetsOverride;
                if (overrideAssets != null)
                {
                    listToUse = overrideAssets;
                }
            }

            feature.ApplyAssets(listToUse, buttonIndex);
        }

        if (parentToDeactivate != null)
        {
            parentToDeactivate.SetActive(false);
        }
    }

    private maskfeature ResolveMaskFeature()
    {
        if (maskFeature != null)
        {
            cachedMaskFeature = maskFeature;
            return maskFeature;
        }

        if (cachedMaskFeature != null)
        {
            return cachedMaskFeature;
        }

        maskfeature[] allFeatures = Resources.FindObjectsOfTypeAll<maskfeature>();
        for (int i = 0; i < allFeatures.Length; i++)
        {
            maskfeature feature = allFeatures[i];
            if (feature == null)
            {
                continue;
            }

            if (!feature.gameObject.scene.IsValid())
            {
                continue;
            }

            cachedMaskFeature = feature;
            return feature;
        }

        return null;
    }

    private void ResetRiddleProgress()
    {
        riddleProgress = 0;
        if (riddleButtons != null)
        {
            for (int i = 0; i < riddleButtons.Length; i++)
            {
                if (riddleButtons[i] != null)
                {
                    riddleButtons[i].interactable = true;
                }
            }
        }
    }

    public void GoBack()
    {
        GameObject target = backTargetOverride;
        if (target == null && backHistory.Count > 0)
        {
            target = backHistory.Pop();
        }

        if (target == null)
        {
            return;
        }

        if (gameSettings != null)
        {
            gameSettings.FadeAndSwitch(() => SwitchNow(target, -1, false));
            return;
        }

        SwitchNow(target, -1, false);
    }

    public bool CanGoBack()
    {
        if (backTargetOverride != null)
        {
            return true;
        }

        return backHistory.Count > 0;
    }

    private void CacheCurrentTargetFromScene()
    {
        currentTarget = null;
        if (buttons == null)
        {
            return;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject target = buttons[i].target;
            if (target != null && target.activeSelf)
            {
                currentTarget = target;
                return;
            }
        }
    }
}

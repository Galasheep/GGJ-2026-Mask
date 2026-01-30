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
    }

    [Header("Buttons mapped to their target prefabs")]
    [SerializeField] private ButtonTarget[] buttons;

    [Header("Optional parent to deactivate on click")]
    [SerializeField] private GameObject parentToDeactivate;

    [Header("Hover alpha effect")]
    [SerializeField] private bool enableHoverAlpha = true;
    [SerializeField, Range(0f, 1f)] private float hoverOnAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float hoverOffAlpha = 0f;

    private GameSettings gameSettings;
    private maskfeature cachedMaskFeature;

    private void OnEnable()
    {
        maskfeature feature = ResolveMaskFeature();
        if (feature != null)
        {
            feature.ApplyList(uiAssets);
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
    }

    private void ActivateTarget(GameObject target, int buttonIndex)
    {
        if (gameSettings != null)
        {
            gameSettings.FadeAndSwitch(() => SwitchNow(target, buttonIndex));
            return;
        }

        SwitchNow(target, buttonIndex);
    }

    private void SwitchNow(GameObject target, int buttonIndex)
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
}

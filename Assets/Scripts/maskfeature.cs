using UnityEngine;
using UnityEngine.UI;

public class maskfeature : MonoBehaviour
{
    [Header("Root to toggle on/off")]
    [SerializeField] private GameObject rootToToggle;

    [Header("Image targets")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image maskImage;

    [Header("Optional toggle buttons")]
    [SerializeField] private Button onButton;
    [SerializeField] private Button[] onButtons;
    [SerializeField] private int[] onButtonMaskIndices;
    [SerializeField] private Button offButton;

    [Header("Optional inventory animation")]
    [SerializeField] private Animator inventoryAnimator;
    [SerializeField] private string outTrigger = "Out";
    [SerializeField] private string inTrigger = "In";
    [SerializeField] private bool disableRootOnIn = true;
    [SerializeField] private float inDisableDelay = 0.0f;

    [Header("Mask feature fade/zoom")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private RectTransform rootRectTransform;
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float zoomDuration = 0.15f;
    [SerializeField] private float zoomFromScale = 1.1f;

    [Header("Optional default state")]
    [SerializeField] private bool startEnabled = false;

    private MaskUiAssetList lastList;
    private int lastMaskIndex = -1;
    private bool buttonsHooked;
    private Coroutine disableRoutine;
    private Coroutine fadeRoutine;
    private GameSettings gameSettings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitializeAll()
    {
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

            feature.RuntimeInitialize();
        }
    }

    private void Awake()
    {
        RuntimeInitialize();
    }

    private void RuntimeInitialize()
    {
        if (gameSettings == null)
        {
            gameSettings = FindFirstObjectByType<GameSettings>();
        }

        if (rootToToggle == null)
        {
            rootToToggle = gameObject;
        }

        if (rootCanvasGroup == null && rootToToggle != null)
        {
            rootCanvasGroup = rootToToggle.GetComponentInChildren<CanvasGroup>(true);
        }

        if (rootRectTransform == null && rootToToggle != null)
        {
            rootRectTransform = rootToToggle.GetComponentInChildren<RectTransform>(true);
        }

        if (!buttonsHooked)
        {
            if (onButton != null)
            {
                onButton.onClick.AddListener(TurnOn);
            }

            if (onButtons != null)
            {
                for (int i = 0; i < onButtons.Length; i++)
                {
                    Button button = onButtons[i];
                    int maskIndex = ResolveOnButtonMaskIndex(i);
                    if (button == null)
                    {
                        continue;
                    }

                    button.onClick.AddListener(() => TurnOnWithMaskIndex(maskIndex));
                }
            }

            if (offButton != null)
            {
                offButton.onClick.AddListener(TurnOff);
            }

            buttonsHooked = true;
        }

        SetActive(startEnabled);
        if (startEnabled)
        {
            ApplyFade(1f, 1f);
        }
    }

    public void ApplyAssets(MaskUiAssetList list, int maskIndex)
    {
        lastMaskIndex = maskIndex;
        ApplyList(list);
    }

    public void ApplyList(MaskUiAssetList list)
    {
        lastList = list;

        if (list == null)
        {
            return;
        }

        if (bgImage != null)
        {
            bgImage.sprite = list.BG;
        }

        if (maskImage != null)
        {
            Sprite maskSprite = GetMaskSprite(list, lastMaskIndex);
            maskImage.sprite = maskSprite;
        }
    }

    public void TurnOn()
    {
        TriggerOut();
        SyncActiveSwitchList();
        ApplyCached();
    }

    public void TurnOnWithMaskIndex(int maskIndex)
    {
        lastMaskIndex = maskIndex;
        TurnOn();
    }

    public void TurnOff()
    {
        TriggerIn();
    }

    public void SetActive(bool active)
    {
        if (rootToToggle != null)
        {
            rootToToggle.SetActive(active);
            return;
        }

        gameObject.SetActive(active);
    }

    public void TriggerOut()
    {
        if (inventoryAnimator == null)
        {
            return;
        }

        SetActive(true);
        CancelDisable();
        StartFadeZoomIn();
        if (!string.IsNullOrEmpty(inTrigger))
        {
            inventoryAnimator.ResetTrigger(inTrigger);
        }

        if (!string.IsNullOrEmpty(outTrigger))
        {
            inventoryAnimator.SetTrigger(outTrigger);
        }
    }

    public void TriggerIn()
    {
        if (inventoryAnimator == null)
        {
            SetActive(false);
            return;
        }

        if (!string.IsNullOrEmpty(outTrigger))
        {
            inventoryAnimator.ResetTrigger(outTrigger);
        }

        if (!string.IsNullOrEmpty(inTrigger))
        {
            inventoryAnimator.SetTrigger(inTrigger);
        }

        if (disableRootOnIn)
        {
            CancelDisable();
            disableRoutine = StartCoroutine(DisableAfterDelay(inDisableDelay));
        }

        StartFadeZoomOut();
    }

    private void CancelDisable()
    {
        if (disableRoutine != null)
        {
            StopCoroutine(disableRoutine);
            disableRoutine = null;
        }
    }

    private void StartFadeZoomIn()
    {
        if (rootCanvasGroup == null && rootRectTransform == null)
        {
            return;
        }

        StartFadeRoutine(0f, 1f, zoomFromScale, 1f);
    }

    private void StartFadeZoomOut()
    {
        if (rootCanvasGroup == null && rootRectTransform == null)
        {
            return;
        }

        StartFadeRoutine(1f, 0f, 1f, 0.98f);
    }

    private void StartFadeRoutine(float fromAlpha, float toAlpha, float fromScale, float toScale)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeZoomRoutine(fromAlpha, toAlpha, fromScale, toScale));
    }

    private System.Collections.IEnumerator FadeZoomRoutine(float fromAlpha, float toAlpha, float fromScale, float toScale)
    {
        ApplyFade(fromAlpha, fromScale);

        float duration = Mathf.Max(fadeDuration, zoomDuration);
        if (duration <= 0f)
        {
            ApplyFade(toAlpha, toScale);
            fadeRoutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float alpha = Mathf.Lerp(fromAlpha, toAlpha, Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, fadeDuration)));
            float scale = Mathf.Lerp(fromScale, toScale, Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, zoomDuration)));
            ApplyFade(alpha, scale);
            yield return null;
        }

        ApplyFade(toAlpha, toScale);
        fadeRoutine = null;
    }

    private void ApplyFade(float alpha, float scale)
    {
        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = alpha;
        }

        if (rootRectTransform != null)
        {
            rootRectTransform.localScale = Vector3.one * scale;
        }
    }

    private System.Collections.IEnumerator DisableAfterDelay(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        SetActive(false);
        disableRoutine = null;
    }

    private void ApplyCached()
    {
        SyncActiveSwitchList();
        if (lastList == null)
        {
            return;
        }

        ApplyAssets(lastList, lastMaskIndex);
    }

    private Sprite GetMaskSprite(MaskUiAssetList list, int maskIndex)
    {
        if (list == null || list.Masks == null)
        {
            return null;
        }

        if (list.Masks.Count == 0)
        {
            return null;
        }

        if (maskIndex < 0 || maskIndex >= list.Masks.Count)
        {
            return list.Masks[0];
        }

        return list.Masks[maskIndex];
    }

    private void SyncActiveSwitchList()
    {
        if (gameSettings == null)
        {
            gameSettings = FindFirstObjectByType<GameSettings>();
        }

        if (gameSettings == null)
        {
            return;
        }

        PrefabSwitchList activeList = gameSettings.GetActiveSwitchList();
        if (activeList == null)
        {
            return;
        }

        MaskUiAssetList assets = activeList.GetUiAssets();
        if (assets != null && assets != lastList)
        {
            lastList = assets;
        }
    }

    private int ResolveOnButtonMaskIndex(int buttonIndex)
    {
        if (onButtonMaskIndices != null &&
            buttonIndex >= 0 &&
            buttonIndex < onButtonMaskIndices.Length)
        {
            return onButtonMaskIndices[buttonIndex];
        }

        return buttonIndex;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Black fade layer (UI panel GameObject)")]
    [SerializeField] private GameObject blackLayer;
    [SerializeField] private float fadeDuration = 0.35f;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private bool isFading;
    private CanvasGroup blackLayerGroup;
    private Graphic blackLayerGraphic;
    private PrefabSwitchList activeSwitchList;

    private void Awake()
    {
        CacheBlackLayerComponents();
        SetAlpha(0f);
        SetRaycastBlock(false);
    }

    private void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackPressed);
        }

        RefreshBackButton();
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackPressed);
        }
    }

    private void CacheBlackLayerComponents()
    {
        if (blackLayer == null)
        {
            return;
        }

        blackLayerGroup = blackLayer.GetComponentInChildren<CanvasGroup>(true);
        if (blackLayerGroup == null)
        {
            blackLayerGroup = blackLayer.AddComponent<CanvasGroup>();
        }

        blackLayerGraphic = blackLayer.GetComponentInChildren<Graphic>(true);
    }

    public void FadeAndSwitch(System.Action onBlack)
    {
        if (blackLayer == null)
        {
            onBlack?.Invoke();
            return;
        }

        if (blackLayerGroup == null && blackLayerGraphic == null)
        {
            CacheBlackLayerComponents();
        }

        if (blackLayerGroup == null && blackLayerGraphic == null)
        {
            onBlack?.Invoke();
            return;
        }

        if (isFading)
        {
            return;
        }

        StartCoroutine(FadeRoutine(onBlack));
    }

    public void SetActiveSwitchList(PrefabSwitchList list)
    {
        activeSwitchList = list;
        RefreshBackButton();
    }

    public void ClearActiveSwitchList(PrefabSwitchList list)
    {
        if (activeSwitchList == list)
        {
            activeSwitchList = null;
        }
        RefreshBackButton();
    }

    public PrefabSwitchList GetActiveSwitchList()
    {
        return activeSwitchList;
    }

    private void HandleBackPressed()
    {
        if (activeSwitchList == null)
        {
            return;
        }

        activeSwitchList.GoBack();
        RefreshBackButton();
    }

    public void RefreshBackButton()
    {
        if (backButton == null)
        {
            return;
        }

        bool shouldShow = activeSwitchList != null && activeSwitchList.CanGoBack();
        backButton.gameObject.SetActive(shouldShow);
    }

    private IEnumerator FadeRoutine(System.Action onBlack)
    {
        isFading = true;

        blackLayer.SetActive(true);
        SetAlpha(0f);
        SetRaycastBlock(true);
        yield return FadeAlpha(0f, 1f, fadeDuration);

        onBlack?.Invoke();

        yield return FadeAlpha(1f, 0f, fadeDuration);
        SetRaycastBlock(false);
        blackLayer.SetActive(false);

        isFading = false;
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(Mathf.Lerp(from, to, t));
            yield return null;
        }

        SetAlpha(to);
    }

    private void SetAlpha(float value)
    {
        if (blackLayerGroup != null)
        {
            blackLayerGroup.alpha = value;
            return;
        }

        if (blackLayerGraphic != null)
        {
            Color c = blackLayerGraphic.color;
            c.a = value;
            blackLayerGraphic.color = c;
        }
    }

    private void SetRaycastBlock(bool block)
    {
        if (blackLayerGroup != null)
        {
            blackLayerGroup.blocksRaycasts = block;
            blackLayerGroup.interactable = block;
        }
    }
}

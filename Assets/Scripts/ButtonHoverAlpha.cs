using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverAlpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField, Range(0f, 1f)] private float hoverOnAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float hoverOffAlpha = 0f;
    private Button cachedButton;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            cachedButton = GetComponent<Button>();
            targetGraphic = cachedButton != null ? cachedButton.targetGraphic : GetComponent<Graphic>();
        }
        else
        {
            cachedButton = GetComponent<Button>();
        }

        SetAlpha(hoverOffAlpha);

        if (cachedButton != null)
        {
            cachedButton.onClick.AddListener(HandleClick);
        }
    }

    private void OnDestroy()
    {
        if (cachedButton != null)
        {
            cachedButton.onClick.RemoveListener(HandleClick);
        }
    }

    public void Configure(float onAlpha, float offAlpha)
    {
        hoverOnAlpha = Mathf.Clamp01(onAlpha);
        hoverOffAlpha = Mathf.Clamp01(offAlpha);
        SetAlpha(hoverOffAlpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetAlpha(hoverOnAlpha);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetAlpha(hoverOffAlpha);
    }

    private void HandleClick()
    {
        SetAlpha(hoverOffAlpha);
    }

    private void SetAlpha(float value)
    {
        if (targetGraphic == null)
        {
            return;
        }

        Color c = targetGraphic.color;
        c.a = value;
        targetGraphic.color = c;
    }
}

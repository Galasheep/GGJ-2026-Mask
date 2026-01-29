using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverAlpha : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Graphic targetGraphic;
    [SerializeField, Range(0f, 1f)] private float hoverOnAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float hoverOffAlpha = 0f;

    private void Awake()
    {
        if (targetGraphic == null)
        {
            Button button = GetComponent<Button>();
            targetGraphic = button != null ? button.targetGraphic : GetComponent<Graphic>();
        }

        SetAlpha(hoverOffAlpha);
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

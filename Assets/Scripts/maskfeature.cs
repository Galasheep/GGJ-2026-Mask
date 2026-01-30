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
    [SerializeField] private Button offButton;

    [Header("Optional inventory animation")]
    [SerializeField] private Animator inventoryAnimator;
    [SerializeField] private string outTrigger = "Out";
    [SerializeField] private string inTrigger = "In";
    [SerializeField] private bool disableRootOnIn = true;
    [SerializeField] private float inDisableDelay = 0.0f;

    [Header("Optional default state")]
    [SerializeField] private bool startEnabled = false;

    private MaskUiAssetList lastList;
    private int lastMaskIndex = -1;
    private bool buttonsHooked;
    private Coroutine disableRoutine;

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
        if (rootToToggle == null)
        {
            rootToToggle = gameObject;
        }

        if (!buttonsHooked)
        {
            if (onButton != null)
            {
                onButton.onClick.AddListener(TurnOn);
            }

            if (offButton != null)
            {
                offButton.onClick.AddListener(TurnOff);
            }

            buttonsHooked = true;
        }

        SetActive(startEnabled);
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

        if (maskImage != null && lastMaskIndex >= 0)
        {
            Sprite maskSprite = GetMaskSprite(list, lastMaskIndex);
            if (maskSprite != null)
            {
                maskImage.sprite = maskSprite;
            }
        }
    }

    public void TurnOn()
    {
        TriggerOut();
        ApplyCached();
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
    }

    private void CancelDisable()
    {
        if (disableRoutine != null)
        {
            StopCoroutine(disableRoutine);
            disableRoutine = null;
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

        if (maskIndex < 0 || maskIndex >= list.Masks.Count)
        {
            return null;
        }

        return list.Masks[maskIndex];
    }
}

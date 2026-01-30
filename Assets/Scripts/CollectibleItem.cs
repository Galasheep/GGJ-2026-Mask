using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectibleItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Inventory")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private string itemId;

    [Header("Hover material")]
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Graphic[] targetGraphics;

    private Material[][] originalRendererMaterials;
    private Material[] originalGraphicMaterials;
    private bool isCollected;

    private void Awake()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindFirstObjectByType<InventoryManager>();
        }

        CacheTargets();
        CacheOriginalMaterials();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isCollected)
        {
            return;
        }

        ApplyHoverMaterial();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isCollected)
        {
            return;
        }

        RestoreOriginalMaterials();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCollected)
        {
            return;
        }

        Collect();
    }

    private void OnDisable()
    {
        RestoreOriginalMaterials();
    }

    private void CacheTargets()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }

        if (targetGraphics == null || targetGraphics.Length == 0)
        {
            targetGraphics = GetComponentsInChildren<Graphic>(true);
        }
    }

    private void CacheOriginalMaterials()
    {
        if (targetRenderers != null)
        {
            originalRendererMaterials = new Material[targetRenderers.Length][];
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer renderer = targetRenderers[i];
                originalRendererMaterials[i] = renderer != null ? renderer.materials : null;
            }
        }

        if (targetGraphics != null)
        {
            originalGraphicMaterials = new Material[targetGraphics.Length];
            for (int i = 0; i < targetGraphics.Length; i++)
            {
                Graphic graphic = targetGraphics[i];
                originalGraphicMaterials[i] = graphic != null ? graphic.material : null;
            }
        }
    }

    private void ApplyHoverMaterial()
    {
        if (hoverMaterial == null)
        {
            return;
        }

        if (targetRenderers != null)
        {
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer renderer = targetRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] materials = renderer.materials;
                bool hasHover = false;
                for (int m = 0; m < materials.Length; m++)
                {
                    if (materials[m] == hoverMaterial)
                    {
                        hasHover = true;
                        break;
                    }
                }

                if (!hasHover)
                {
                    Material[] newMaterials = new Material[materials.Length + 1];
                    for (int m = 0; m < materials.Length; m++)
                    {
                        newMaterials[m] = materials[m];
                    }

                    newMaterials[newMaterials.Length - 1] = hoverMaterial;
                    renderer.materials = newMaterials;
                }
            }
        }

        if (targetGraphics != null)
        {
            for (int i = 0; i < targetGraphics.Length; i++)
            {
                Graphic graphic = targetGraphics[i];
                if (graphic != null)
                {
                    graphic.material = hoverMaterial;
                }
            }
        }
    }

    private void RestoreOriginalMaterials()
    {
        if (targetRenderers != null && originalRendererMaterials != null)
        {
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                Renderer renderer = targetRenderers[i];
                Material[] materials = originalRendererMaterials[i];
                if (renderer != null && materials != null)
                {
                    renderer.materials = materials;
                }
            }
        }

        if (targetGraphics != null && originalGraphicMaterials != null)
        {
            for (int i = 0; i < targetGraphics.Length; i++)
            {
                Graphic graphic = targetGraphics[i];
                if (graphic != null)
                {
                    graphic.material = originalGraphicMaterials[i];
                }
            }
        }
    }

    private void Collect()
    {
        isCollected = true;
        RestoreOriginalMaterials();

        if (inventoryManager != null)
        {
            inventoryManager.Collect(itemId);
        }

        Destroy(gameObject);
    }
}

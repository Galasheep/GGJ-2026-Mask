using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryEntry
    {
        public string itemId;
        public GameObject iconObject;
    }

    [Header("Inventory items (id -> icon)")]
    [SerializeField] private InventoryEntry[] items;
    [SerializeField] private bool deactivateIconsOnStart = true;

    private readonly Dictionary<string, GameObject> itemLookup =
        new Dictionary<string, GameObject>();

    private void Awake()
    {
        BuildLookup();

        if (deactivateIconsOnStart)
        {
            SetAllIconsActive(false);
        }
    }

    public bool Collect(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return false;
        }

        if (!itemLookup.TryGetValue(itemId, out GameObject icon) || icon == null)
        {
            return false;
        }

        icon.SetActive(true);
        return true;
    }

    public void SetAllIconsActive(bool active)
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            GameObject icon = items[i].iconObject;
            if (icon != null)
            {
                icon.SetActive(active);
            }
        }
    }

    private void BuildLookup()
    {
        itemLookup.Clear();

        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            string id = items[i].itemId;
            GameObject icon = items[i].iconObject;
            if (string.IsNullOrEmpty(id) || icon == null)
            {
                continue;
            }

            itemLookup[id] = icon;
        }
    }
}

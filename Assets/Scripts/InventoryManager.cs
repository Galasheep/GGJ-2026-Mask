using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [System.Serializable]
    public struct InventoryEntry
    {
        public string itemId;
        public GameObject iconObject;
        [Tooltip("Optional switch list target to show when collecting (normal fade like other switches). Leave empty to skip.")]
        public GameObject displayTarget;
    }

    [Header("Inventory items (id -> icon)")]
    [SerializeField] private InventoryEntry[] items;
    [SerializeField] private bool deactivateIconsOnStart = true;

    [Header("Collection display")]
    [Tooltip("Switch list that owns the display targets. Used for fade + switch when collecting an item with a display target.")]
    [SerializeField] private PrefabSwitchList collectionSwitchList;

    private readonly Dictionary<string, GameObject> itemLookup =
        new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> displayTargetLookup =
        new Dictionary<string, GameObject>();

    private void Awake()
    {
        BuildLookup();

        if (deactivateIconsOnStart)
        {
            SetAllIconsActive(false);
        }

        if (collectionSwitchList == null)
        {
            collectionSwitchList = FindFirstObjectByType<PrefabSwitchList>();
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

        if (collectionSwitchList != null &&
            displayTargetLookup.TryGetValue(itemId, out GameObject displayTarget) &&
            displayTarget != null)
        {
            collectionSwitchList.SwitchToTarget(displayTarget);
        }

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
        displayTargetLookup.Clear();

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

            if (items[i].displayTarget != null)
            {
                displayTargetLookup[id] = items[i].displayTarget;
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks which riddles have been solved by ID. Use the same ID on PrefabSwitchList (riddle)
/// and on CollectibleItem (required riddle) to gate items behind riddles.
/// </summary>
public class RiddleManager : MonoBehaviour
{
    public static RiddleManager Instance { get; private set; }

    private readonly HashSet<string> solvedRiddleIds = new HashSet<string>();

    [Tooltip("Keep solved state when loading other scenes so items stay pickable after solving.")]
    [SerializeField] private bool persistAcrossScenes = true;

    /// <summary>Use this before marking or checking riddles so a manager exists even if not in the scene.</summary>
    public static RiddleManager EnsureExists()
    {
        if (Instance != null)
        {
            return Instance;
        }

        RiddleManager existing = FindFirstObjectByType<RiddleManager>();
        if (existing != null)
        {
            return existing;
        }

        GameObject go = new GameObject("RiddleManager");
        RiddleManager created = go.AddComponent<RiddleManager>();
        return created;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static string NormalizeId(string id)
    {
        return id == null ? string.Empty : id.Trim().ToLowerInvariant();
    }

    /// <summary>Marks a riddle as solved. Call this when the player completes a riddle (e.g. from PrefabSwitchList).</summary>
    public void MarkRiddleSolved(string riddleId)
    {
        string key = NormalizeId(riddleId);
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        solvedRiddleIds.Add(key);
    }

    /// <summary>Returns true if the riddle with the given ID has been solved.</summary>
    public bool IsRiddleSolved(string riddleId)
    {
        string key = NormalizeId(riddleId);
        if (string.IsNullOrEmpty(key))
        {
            return true;
        }

        return solvedRiddleIds.Contains(key);
    }
}

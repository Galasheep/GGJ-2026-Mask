using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaskUiAssetList", menuName = "Mask/Mask UI Asset List")]
public class MaskUiAssetList : ScriptableObject
{
    [Header("UI assets")]
    public Sprite BG;

    [Header("Mask list")]
    public List<Sprite> Masks = new List<Sprite>();

    [Header("Music per mask (index matches Masks; leave empty to skip)")]
    [Tooltip("Music to play when this mask is shown. One entry per mask image.")]
    public List<AudioClip> MaskMusic = new List<AudioClip>();

    /// <summary>Returns the music clip for the given mask index, or null if none.</summary>
    public AudioClip GetMusicForMaskIndex(int index)
    {
        if (index < 0 || index >= MaskMusic.Count)
        {
            return null;
        }

        return MaskMusic[index];
    }
}

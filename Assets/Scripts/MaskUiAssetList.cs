using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MaskUiAssetList", menuName = "Mask/Mask UI Asset List")]
public class MaskUiAssetList : ScriptableObject
{
    [Header("UI assets")]
    public Sprite BG;

    [Header("Mask list")]
    public List<Sprite> Masks = new List<Sprite>();
}

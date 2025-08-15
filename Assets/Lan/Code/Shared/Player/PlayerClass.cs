using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Class")]
public class PlayerClass : ScriptableObject
{
    public string classId = "default";                 // no futuro: “mage”, “warrior” etc.
    public RuntimeAnimatorController animator;         // Player_Default.controller
    public Sprite defaultIdleSprite;                   // frame idle (fallback)
}

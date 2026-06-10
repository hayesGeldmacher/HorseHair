using UnityEngine;

[CreateAssetMenu(fileName = "Cursors", menuName = "PointAndClick/Cursors")]
public class CursorsScript : ScriptableObject
{
    [Tooltip("The type of objects this cursor is for.")]
    public CursorType cursorType;
    public Texture2D[] frames;
    [Tooltip("The frame rate of the cursor animation (lower is faster).")]
    public float frameRate;
    [Tooltip("The center point of the cursor hotspot.")]
    public Vector2 offset;
}

using UnityEngine;

public class DetectCursor : MonoBehaviour
{
    public static void ChangeColourOnDetection(GameObject gameObject, out Color oldColour)
    {
        var sprite = gameObject.GetComponent<SpriteRenderer>();
        oldColour = sprite.color;
        sprite.color = new Color(0, 255, 0); //pure green
    }

    public static void ChangeColourBack(GameObject gameObject, Color oldColour)
    {
        gameObject.GetComponent<SpriteRenderer>().color = oldColour;
    }
}

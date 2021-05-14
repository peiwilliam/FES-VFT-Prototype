using UnityEngine;

public class DetectCursor
{
    public static void ChangeColourOnDetection(GameObject gameObject, out Color oldColour)
    {
        var sprite = gameObject.GetComponent<SpriteRenderer>();
        oldColour = sprite.color;
        sprite.color = Color.green; //pure green
    }

    public static void ChangeColourBack(GameObject gameObject, Color oldColour)
    {
        gameObject.GetComponent<SpriteRenderer>().color = oldColour;
    }
}

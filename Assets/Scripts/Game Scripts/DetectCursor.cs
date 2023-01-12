using UnityEngine;

/// <summary>
/// This static class is just responsible for changint the colour back and forth between the original colour and pure green when
/// the player is inside the correct target circle. Doesn't apply in the Target game.
/// </summary>
public static class DetectCursor
{
    /// <summary>
    /// This method is responsible for changing the colour to green when the player is inside the correct circle.
    /// </summary>
    public static void ChangeColourOnDetection(GameObject gameObject, out Color oldColour)
    {
        var sprite = gameObject.GetComponent<SpriteRenderer>();
        oldColour = sprite.color;
        sprite.color = Color.green; //pure green
    }

    /// <summary>
    /// This method is responsible for changing the colour back to the original colour when the player leaves the circle.
    /// </summary>
    public static void ChangeColourBack(GameObject gameObject, Color oldColour) => gameObject.GetComponent<SpriteRenderer>().color = oldColour;
}

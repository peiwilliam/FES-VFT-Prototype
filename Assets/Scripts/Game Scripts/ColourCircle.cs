using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourCircle : MonoBehaviour
{
    private Color _oldColour;
    
    private void OnTriggerEnter2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour);
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCursor : MonoBehaviour
{
    private Color _oldColor;
    
    private void OnTriggerEnter2D(Collider2D collider) 
    {
        var sprite = gameObject.GetComponent<SpriteRenderer>();
        _oldColor = sprite.color;
        gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 255, 0);
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        gameObject.GetComponent<SpriteRenderer>().color = _oldColor;
    }
}

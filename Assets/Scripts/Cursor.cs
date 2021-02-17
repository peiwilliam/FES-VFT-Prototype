using System.Collections;
using System.Collections.Generic;
using WiimoteLib;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    private Wiimote _wiiDevice;

    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    
    // Update is called once per frame
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        //debugging using mouse
        var pos = new Vector2(transform.position.x, transform.position.y);
        pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
        pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.width * _maxX, _minY, _maxY);
        transform.position = pos;

        // var pos = new Vector2(transform.position.x, transform.position.y);
        // pos.x = Mathf.Clamp();
    }
}

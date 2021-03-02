using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    
    //private WiiBoard _wiiBoard;

    private void Start() 
    {
        //_wiiBoard = FindObjectOfType<WiiBoard>();
    }
    
    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        if ((bool)FindObjectOfType<WiiBoard>())
        {
            // var pos = new Vector2(transform.position.x, transform.position.y);
            // var cop = _wiiBoard.GetSensorValues();
            // pos.x = Mathf.Clamp(cop[0], _minX, _maxX);
            // pos.y = Mathf.Clamp(cop[1], _minY, _maxY);
            // transform.position = pos;
        }
        else
        {
            // debugging using mouse
            var pos = new Vector2(transform.position.x, transform.position.y);
            pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
            pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.width * _maxX, _minY, _maxY);
            transform.position = pos;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _minX = 0f;
    [SerializeField] private float _maxX = 2f*5f*16f/9f; //2*height*aspect ratio
    [SerializeField] private float _minY = 0f;
    [SerializeField] private float _maxY = 5f*2f; //2*camera size
    
    // Update is called once per frame
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var pos = new Vector2(transform.position.x, transform.position.y);
        pos.x = Mathf.Clamp(Input.mousePosition.x / Screen.width * _maxX, _minX, _maxX);
        pos.y = Mathf.Clamp(Input.mousePosition.y / Screen.width * _maxX, _minY, _maxY);
        transform.position = pos;
    }
}

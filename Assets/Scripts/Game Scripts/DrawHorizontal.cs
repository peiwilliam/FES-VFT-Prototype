using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawHorizontal : MonoBehaviour
{
    [SerializeField] private float _lineWidth = 0.05f;
 
    private LineRenderer _horizontal;

    private void Start() 
    {
        _horizontal = GetComponent<LineRenderer>();
        SetupGrid();
    }

    private void SetupGrid()
    {
        _horizontal.widthMultiplier = _lineWidth;
        _horizontal.positionCount = 2; //just a straight line

        _horizontal.SetPosition(0, new Vector3(0, Camera.main.transform.position.y));
        _horizontal.SetPosition(1, new Vector3(2f*5f*16f/9f, Camera.main.transform.position.y));
    }

// we only need this drawgizmos method once, don't need in both draw horizontal and drawvertical
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 10 is max height of game and 
        Gizmos.DrawLine(new Vector3(Camera.main.transform.position.x, 0), new Vector3(Camera.main.transform.position.x, 10f));
        Gizmos.DrawLine(new Vector3(0, Camera.main.transform.position.y), new Vector3(2f*5f*16f/9f, Camera.main.transform.position.y));
    }
#endif
}

using UnityEngine;

/// <summary>
/// This class is responsible for drawing the horizontal line seen in the hunting game. this is separate from DrawVertical because
/// there can only be one LineRenderer object attached to an object at any given moment and  drawing the vertical and horizontal both
/// at the same time creates unwanted diagonals.
/// </summary>
public class DrawHorizontal : MonoBehaviour
{
    [Tooltip("Line width of the horizontal line marking the top and bottom half of the game screen")]
    [SerializeField] private float _lineWidth = 0.05f;
 
    private LineRenderer _horizontal;

    private void Start() //runs at the beginning when the object is instantiated
    {
        _horizontal = GetComponent<LineRenderer>();
        SetupGrid();
    }

    private void SetupGrid() //draws the horizontal line with line renderer
    {
        _horizontal.widthMultiplier = _lineWidth;
        _horizontal.positionCount = 2; //just a straight line

        _horizontal.SetPosition(0, new Vector3(0, Camera.main.transform.position.y));
        _horizontal.SetPosition(1, new Vector3(2f*5f*16f/9f, Camera.main.transform.position.y));
    }

// we only need this drawgizmos method once, don't need in both draw horizontal and drawvertical
#if UNITY_EDITOR
    private void OnDrawGizmos() //This method is for drawing the position of the horizontal and vertical line while in editor
    {
        // 10 is max height of game and 
        Gizmos.DrawLine(new Vector3(Camera.main.transform.position.x, 0), new Vector3(Camera.main.transform.position.x, 10f));
        Gizmos.DrawLine(new Vector3(0, Camera.main.transform.position.y), new Vector3(2f*5f*16f/9f, Camera.main.transform.position.y));
    }
#endif
}

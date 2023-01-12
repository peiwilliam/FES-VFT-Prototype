using UnityEngine;

/// <summary>
/// This class is responsible for drawing the vertical line seen in the hunting game.
/// </summary>
public class DrawVertical : MonoBehaviour
{
    [Tooltip("Line width of the vertical line marking the left and bottom half of the game screen")]
    [SerializeField] private float _lineWidth = 0.05f;

    private LineRenderer _vertical;

    private void Start() //runs at the beginning when the object is instantiated
    {
        _vertical = GetComponent<LineRenderer>();
        SetupGrid();
    }

    private void SetupGrid() //draws the vertical line with line renderer
    {
        _vertical.widthMultiplier = _lineWidth;
        _vertical.positionCount = 2; //just a straight line

        _vertical.SetPosition(0, new Vector3(Camera.main.transform.position.x, 0));
        _vertical.SetPosition(1, new Vector3(Camera.main.transform.position.x, 10f));
    }
}

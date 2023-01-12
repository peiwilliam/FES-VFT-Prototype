using UnityEngine;

/// <summary>
/// This is class is responsible for drawing the ellipse seen in the ellipse game.
/// </summary>
public class Ellipse : MonoBehaviour
{
    [Tooltip("The number of vertices used to draw the ellipse")]
    [SerializeField] private int _vertexCount = 60;
    [Tooltip("Line width of the ellipse that is drawn on screen")]
    [SerializeField] private float _lineWidth = 0.05f;
    [Tooltip("Radius of the x axis of the ellipse")]
    [SerializeField] private float _xRadius = 7;
    [Tooltip("Radius of the y axis of the ellipse")]
    [SerializeField] private float _yRadius = 4;
 
    private LineRenderer _lineRenderer; //The linerenderer object that will be drawing out theellipse

    private void Awake() //Runs at the instantiation of the object. This needs to be awake not start, game doesn't work in build if it's start even though it works in editor
    {
        _lineRenderer = GetComponent<LineRenderer>();
        SetupEllipse();
    }
    
    private void SetupEllipse() //draws the ellipse. The ellipse is approximated b using a set number of vertices.
    {
        _lineRenderer.widthMultiplier = _lineWidth;
        _lineRenderer.positionCount = _vertexCount;

        var deltaTheta = (2f * Mathf.PI) / _vertexCount;
        var theta = 0f; //size/(2*size*aspect ratio)

        for (var i = 0; i < _lineRenderer.positionCount; i++)
        {
            var pos = new Vector3(_xRadius * Mathf.Cos(theta), _yRadius * Mathf.Sin(theta), 0f);
            pos.x += Camera.main.transform.position.x; //bring the ellipse to where the camera centre is 5f*16f/9f height*aspect ratio
            pos.y += Camera.main.transform.position.y; //5f height
            _lineRenderer.SetPosition(i, pos);
            theta += deltaTheta;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos() //this method only runs at in editor so that we can see what the ellipse will look like in game.
    {
        var deltaTheta = (2f * Mathf.PI) / _vertexCount;
        var theta = 0f; //size/(2*size*aspect ratio)
        //since it draws from a reference point, set reference point to very right side of the ellipse so it starts drawing from there
        var oldPos = new Vector3(Camera.main.transform.position.x + _xRadius, Camera.main.transform.position.y); 

        for (var i = 0; i < _vertexCount + 1; i++)
        {
            //need to add by centreX and centreY to shift the drawing to the centre
            var pos = new Vector3(_xRadius * Mathf.Cos(theta) + Camera.main.transform.position.x, _yRadius * Mathf.Sin(theta) + Camera.main.transform.position.y, 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }
#endif
}

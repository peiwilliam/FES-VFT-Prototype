using UnityEngine;

public class Ellipse : MonoBehaviour
{
    [SerializeField] private int _vertexCount = 60; 
    [SerializeField] private float _lineWidth = 0.05f;
    [SerializeField] private float _xRadius = 7;
    [SerializeField] private float _yRadius = 4;
 
    private LineRenderer _lineRenderer;

    private void Start() 
    {
        _lineRenderer = GetComponent<LineRenderer>();
        SetupEllipse();
    }
    
    private void SetupEllipse()
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

    public float[] GetRadii()
    {
        var radii = new float[] {_xRadius, _yRadius};
        return radii;
    }

    public float[] GetCentre()
    {
        var centre = new float[] {Camera.main.transform.position.x, Camera.main.transform.position.y};
        return centre;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
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

using UnityEngine;

public class MovingCircle : MonoBehaviour
{
    [SerializeField] private float _circleVelocity = 3f;

    private Ellipse _ellipse;
    private Vector3[] _ellipsePositions;
    private LineRenderer _lineRenderer;
    private Color _oldColour;
    private int _positionIndex;
    private int _direction;

    private void Start() 
    {
        _ellipse = FindObjectOfType<Ellipse>();

        _lineRenderer = _ellipse.GetComponent<LineRenderer>();
        var positions = new Vector3[_lineRenderer.positionCount];
        _lineRenderer.GetPositions(positions); //pos has an out on it, so values are stored within pos only in the scope of the method
        _ellipsePositions = positions;

        _direction = Random.Range(0, 2); //0 is clockwise, 1 is counterclockwise

        if (_direction == 1)
            _positionIndex = 0;
        else
            _positionIndex = _lineRenderer.positionCount - 1;
    }

    private void Update() 
    {
        MoveCircle();
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour);
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour);
    }
    
    private void MoveCircle()
    { 
        if (_direction == 1)
        {
            if (_positionIndex <= _ellipsePositions.Length - 1)
            {
                var targetPosition = NewPosition();

                if (targetPosition == transform.position)
                    _positionIndex++;
            }
            else
                _positionIndex = 0;
        }
        else
        {
            if (_positionIndex >= 0)
            {
                var targetPosition = NewPosition();

                if (targetPosition == transform.position)
                    _positionIndex--;
            }
            else
                _positionIndex = _lineRenderer.positionCount - 1;
        }
    }

    private Vector3 NewPosition()
    {
        var targetPosition = _ellipsePositions[_positionIndex];
        var movementThisFrame = _circleVelocity * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, movementThisFrame);
        return targetPosition;
    }
}

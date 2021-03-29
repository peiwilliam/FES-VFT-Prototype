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
        var gameSession = FindObjectOfType<GameSession>();
        _ellipse = gameSession.Ellipse;
        _lineRenderer = gameSession.LineRenderer;
        _ellipsePositions = gameSession.Positions;
        _positionIndex = gameSession.EllipseIndex;

        _direction = Random.Range(0, 2); //0 is clockwise, 1 is counterclockwise
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

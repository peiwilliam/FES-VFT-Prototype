using System.Collections;
using UnityEngine;

public class MovingCircle : MonoBehaviour
{
    [SerializeField] private float _circleVelocity = 3f;
    [SerializeField] private float _deltaTimeScore = 0.2f;
    [SerializeField] private float _gracePeriod = 1f;
    [SerializeField] private int _score = 0;

    private Ellipse _ellipse;
    private Vector3[] _ellipsePositions;
    private LineRenderer _lineRenderer;
    private Color _oldColour;
    private int _positionIndex;
    private int _direction;
    private Coroutine _scoreIncreaseCoroutine;
    private Coroutine _scoreDecreaseCoroutine;
    private bool _isDecreasing = false;

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

        if (_isDecreasing)
        {
            StopCoroutine(_scoreDecreaseCoroutine);
            _isDecreasing = false;
        }
            
        _scoreIncreaseCoroutine = StartCoroutine(IncreaseScore());
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour);

        StopCoroutine(_scoreIncreaseCoroutine);
        _scoreDecreaseCoroutine = StartCoroutine(DecreaseScore());
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

    private IEnumerator IncreaseScore()
    {
        while (true)
        {
            _score++;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    private IEnumerator DecreaseScore()
    {
        _isDecreasing = true;

        yield return new WaitForSeconds(_gracePeriod);

        while (true)
        {
            _score--;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    public int GetScore() => _score;
}

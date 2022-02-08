using System.Collections;
using UnityEngine;

public class MovingCircle : MonoBehaviour
{
    [Tooltip("How quickly the circle moves along the ellipse")]
    [SerializeField] private float _circleVelocity = 3f;
    [Tooltip("How quickly the score increases and decreases")]
    [SerializeField] private float _deltaTimeScore = 0.2f;
    [Tooltip("Initial grace period at the start of the game to give the player time to get use to the game and be able to get to target")]
    [SerializeField] private float _startingGracePeriod = 5f;
    [Tooltip("If the player goes out of the circle, this is how long the player has before they start losing points")]
    [SerializeField] private float _gracePeriod = 1f;
    [Tooltip("For debugging purposes only, the score that the player has achieved")]
    [SerializeField] private int _score = 0;
    [Tooltip("For debugging purposes only, whether or not the score is decreasing")]
    [SerializeField] private bool _isDecreasing = false;
    
    private int _positionIndex;
    private int _direction;
    private Ellipse _ellipse;
    private Vector3[] _ellipsePositions;
    private LineRenderer _lineRenderer;
    private Color _oldColour;
    private Coroutine _scoreIncreaseCoroutine;
    private Coroutine _scoreDecreaseCoroutine;
    private Coroutine _initialGracePeriod;

    private void Start() 
    {
        var gameSession = FindObjectOfType<GameSession>();
        _ellipse = gameSession.Ellipse;
        _lineRenderer = gameSession.LineRenderer;
        _ellipsePositions = gameSession.Positions;
        _positionIndex = gameSession.EllipseIndex;

        _direction = Random.Range(0, 2); //0 is clockwise, 1 is counterclockwise

        _initialGracePeriod = StartCoroutine(StartOfGame());
    }

    private void Update() 
    {
        MoveCircle();
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour);

        if (_initialGracePeriod != null)
        {
            StopCoroutine(_initialGracePeriod);
            _initialGracePeriod = null;
        }

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
        var movementThisFrame = _circleVelocity * Time.deltaTime; //don't use realtime for this since it depend son the game frames
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, movementThisFrame);
        return targetPosition;
    }

    private IEnumerator IncreaseScore()
    {
        while (true)
        {
            _score++;
            yield return new WaitForSecondsRealtime(_deltaTimeScore);
        }
    }

    private IEnumerator DecreaseScore()
    {
        _isDecreasing = true;

        yield return new WaitForSecondsRealtime(_gracePeriod);

        while (true)
        {
            _score--;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    private IEnumerator StartOfGame()
    {
        yield return new WaitForSeconds(_startingGracePeriod);

        while (true)
        {
            _score--;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    public int GetScore() => _score;
}
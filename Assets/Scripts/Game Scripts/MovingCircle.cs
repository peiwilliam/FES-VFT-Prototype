using System.Collections;
using UnityEngine;

/// <summary>
/// This class is responsible for determing the behaviour of the moving circle seen in the Ellipse game.
/// </summary>
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
    
    private int _positionIndex; //this is the starting index of the moving circle on the ellipse when the game starts
    private int _direction; //determines the direction that the circle will be moving
    private Ellipse _ellipse; //the ellipse object
    private Vector3[] _ellipsePositions; //all of the positions of the vertices in the ellipse
    private LineRenderer _lineRenderer; //the linerenderer object associated with the ellipse
    private Color _oldColour; //for keeping track of the old colour when the colour of the circle changes
    private Coroutine _scoreIncreaseCoroutine; //coroutine responsible for increaseing the score when the player is in the circle
    private Coroutine _scoreDecreaseCoroutine; //coroutine responsible for decreasing the score when the player is outside the circle
    private Coroutine _initialGracePeriod; //coroutine for the initial grace period at the start of the game to get to the circle

    private void Start() //run once at the beginning when the object is instantiated
    {
        var gameSession = FindObjectOfType<GameSession>();
        _ellipse = gameSession.Ellipse;
        _lineRenderer = gameSession.LineRenderer;
        _ellipsePositions = gameSession.Positions;
        _positionIndex = gameSession.EllipseIndex;

        _direction = Random.Range(0, 2); //0 is clockwise, 1 is counterclockwise

        _initialGracePeriod = StartCoroutine(StartOfGame()); //start this coroutine immediately
    }

    private void Update() //runs at every frame update
    {
        MoveCircle();
    }

    private void OnTriggerEnter2D(Collider2D collider) //handles what happens when another game obejct collides with the circle
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour); //change the colour to green

        if (_initialGracePeriod != null) //stop the initial coroutine and set it to null, stopping a coroutine doesn't make it null automatically
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

    private void OnTriggerExit2D(Collider2D collider) //handles what happens when another game obejct exits the circle
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour); //change it back to the original colour

        StopCoroutine(_scoreIncreaseCoroutine);
        _scoreDecreaseCoroutine = StartCoroutine(DecreaseScore());
    }
    
    private void MoveCircle() //this method is responsible for constantly moving the circle towards the next vertex
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
                _positionIndex = 0; //start back at the beginning once we've gone to the end of the vertices
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
                _positionIndex = _lineRenderer.positionCount - 1; //start back at the end once we've gone to the beginning of the vertices
        }
    }

    private Vector3 NewPosition() // gets the target vertex location and makes the circle move towards that location
    {
        var targetPosition = _ellipsePositions[_positionIndex];
        var movementThisFrame = _circleVelocity * Time.deltaTime; //don't use realtime for this since it depends on the game frames
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, movementThisFrame);
        return targetPosition;
    }

    private IEnumerator IncreaseScore() //coroutine for increasing the score while the player is in the circle
    {
        while (true)
        {
            _score++;
            yield return new WaitForSecondsRealtime(_deltaTimeScore);
        }
    }

    private IEnumerator DecreaseScore() //coroutine for decreasign the score while the player is outsid the circle
    {
        _isDecreasing = true;

        yield return new WaitForSecondsRealtime(_gracePeriod); //there is an initial grace period before the score starts decreasing

        while (true)
        {
            _score--;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    private IEnumerator StartOfGame() //this coroutine is only active at the start of the game and then never runs again.
    {
        yield return new WaitForSeconds(_startingGracePeriod);

        while (true)
        {
            _score--;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    /// <summary>
    /// This method is for getting what the current player score is in the game. Only used to get total score in GameSession.
    /// </summary>
    public int GetScore() => _score;
}
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
    [SerializeField] private float _deltaTimeScore = 0.15f;
    [Tooltip("The time needed to stay in the centre of the target to get the score multiplier")]
    [SerializeField] private float _timeNeededForMultiplier = 3f;
    [Tooltip("For debugging purposes only, the score that the player has achieved")]
    [SerializeField] private int _score = 0;
    [Tooltip("For debugging purposes only, shows whether or not the player has achieved the time needed to get the score multiplier")]
    [SerializeField] private bool _multiplyScore;
    
    private int _positionIndex; //this is the starting index of the moving circle on the ellipse when the game starts
    private int _direction; //determines the direction that the circle will be moving
    private Ellipse _ellipse; //the ellipse object
    private Vector3[] _ellipsePositions; //all of the positions of the vertices in the ellipse
    private LineRenderer _lineRenderer; //the linerenderer object associated with the ellipse
    private Color _oldColour; //for keeping track of the old colour when the colour of the circle changes
    private Coroutine _scoreIncreaseCoroutine; //coroutine responsible for increaseing the score when the player is in the circle
    private Coroutine _multiplier; //coroutine responsible for applying a multiplier on the score increase rate when the player stays in the circle

    private void Awake() //run once at the beginning when the object is instantiated, needs to be awake so that any necessary variables are instantiated before any coroutines start
    {
        var gameSession = FindObjectOfType<GameSession>();
        _ellipse = gameSession.Ellipse;
        _lineRenderer = gameSession.LineRenderer;
        _ellipsePositions = gameSession.Positions;
        _positionIndex = gameSession.EllipseIndex;

        _direction = Random.Range(0, 2); //0 is clockwise, 1 is counterclockwise
    }

    private void Update() //runs at every frame update
    {
        MoveCircle();
    }

    private void OnTriggerEnter2D(Collider2D collider) //handles what happens when another game obejct collides with the circle
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour); //change the colour to green
        _scoreIncreaseCoroutine = StartCoroutine(IncreaseScore());
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (_multiplier == null)
            _multiplier = StartCoroutine(Multiplier());
    }

    private void OnTriggerExit2D(Collider2D collider) //handles what happens when another game obejct exits the circle
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour); //change it back to the original colour

        StopCoroutine(_scoreIncreaseCoroutine);
        if (_multiplier != null)
        {
            StopCoroutine(_multiplier);
            _multiplier = null;
            _multiplyScore = false;
        }
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
            if (!_multiplyScore)
                _score++;
            else
                _score += 3;

            yield return new WaitForSecondsRealtime(_deltaTimeScore);
        }
    }

    private IEnumerator Multiplier() //coroutine for handling when the multiplier should be applied
    {
        yield return new WaitForSecondsRealtime(_timeNeededForMultiplier); //need to stay inside the circle for a couple seconds to trigger multiplier

        _multiplyScore = true;
    }

    /// <summary>
    /// This method is for getting what the current player score is in the game. Only used to get total score in GameSession.
    /// </summary>
    public int GetScore() => _score;
}
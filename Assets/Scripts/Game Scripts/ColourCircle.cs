using System.Collections;
using UnityEngine;

/// <summary>
/// This class is responsible for controlling the behaviour of the colour circles in the colour matching game
/// </summary>
public class ColourCircle : MonoBehaviour
{
    [Tooltip("The time period at which the highest possible score the player can get decreases")]
    [SerializeField] private float _deltaTimeScore = 0.25f;
    [Tooltip("The amount of time allowed to get to a circle before the highest possible score starts to decrease")]
    [SerializeField] private float _gettingToCircleBuffer = 5f;
    [Tooltip("The maximum score per circle")]
    [SerializeField] private int _maxScore = 250;
    [Tooltip("For debugging purposes only: Shows when the score is decreasing")]
    [SerializeField] private bool _isDecreasing;
    [Tooltip("For debugging purposes only: Shows when the cursor has entered the target circle")]
    [SerializeField] private bool _hasEntered;

    private float _actualScore;
    private float _totalDuration;
    private float _timeToGetScore; //How long players need to stay in the circle to get points
    private float _timeLeftToGetScore; //Actually how much time left for the target
    private Color _oldColour; //Just for storing what the original colour is when it switches to green when the player is correct
    private Coroutine _enterCircle; //Coroutine responsible for determining how much time the player has left before they get points
    private Coroutine _exitCircle; //Coroutine responsible for adding to the amount of time needed to stay in the circle if player goes out of the circle
    private Coroutine _gettingToCircle; //Coroutine responsible for giving the players a grace period before they start losing points
    private GameSession _gameSession; //Get the gamesession object for reference values.

    private void Awake() //run when the object is instantiated
    {
        _actualScore = _maxScore;
        _totalDuration = PlayerPrefs.GetInt("Duration of Target", 10);
        _timeToGetScore = PlayerPrefs.GetInt("Duration to Get Points", 3);
        _timeLeftToGetScore = _timeToGetScore;
        _gameSession = FindObjectOfType<GameSession>();
    }

    private void Update() //runs at every frame update, only used to update any circles that just switched from target to untagged
    {
        if (gameObject.tag != "Untagged") //for any circle without the target tag, don't do anything during update
            return;

        if (_gettingToCircle != null) //_gettingToCircle is always not null, so perfect for resets
            StopAllCoroutines();
            
        //if the circle is green and the circle has just been switched to untagged, change the colour back to original colour
        if (gameObject.GetComponent<SpriteRenderer>().color == Color.green)
            DetectCursor.ChangeColourBack(gameObject, _oldColour);
    }

    private void OnTriggerEnter2D(Collider2D collider) //handles what happens when another game obejct collides with the circle
    {
        if (gameObject.GetComponent<ColourCircle>() != _gameSession.TargetColourCircle) //if the cursor isn't in the right target, don't do anything
            return;

        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour); //change the colour to green when cursor in right target

        _hasEntered = true; // tells the game that the player has entered at least once.

        if (_gettingToCircle != null) //need to add this account for when cursor is on a target when game starts
            StopCoroutine(_gettingToCircle);

        if (_isDecreasing)
        {
            StopCoroutine(_exitCircle);
            _isDecreasing = false;
        }

        _enterCircle = StartCoroutine(EnterCircle());

    }

    private void OnTriggerExit2D(Collider2D collider) //handles what happens when another game obejct exits the circle
    {
        if (_enterCircle == null) //we want to just exit this method if the enterCircle coroutine isn't running
            return;
        
        DetectCursor.ChangeColourBack(gameObject, _oldColour); //change the colour back to the original colour

        StopCoroutine(_enterCircle);
        _exitCircle = StartCoroutine(ExitCircle());
    }

    private IEnumerator EnterCircle() //handles the time the player has left before they get points
    {
        while (_timeLeftToGetScore > 0)
        {
            _timeLeftToGetScore -= Time.unscaledDeltaTime;

            yield return null;
        }

        if (_timeLeftToGetScore <= 0) //reset the circle back to starting conditions
        {
            _gameSession.ConditionColourMet = true;
            _hasEntered = false;
            gameObject.tag = "Untagged";
        }
    }

    private IEnumerator ExitCircle() //handles the addition of time and subtraction of the score when the player goes out of the circle
    {
        _isDecreasing = true;

        while (true)
        {
            _actualScore -= _maxScore/(_totalDuration/_deltaTimeScore);

            if (_timeLeftToGetScore > 0) //add time if not completed time inside circle
                _timeLeftToGetScore += 0.0625f;

            yield return new WaitForSecondsRealtime(_deltaTimeScore);
        }
    }

    private IEnumerator GettingToCircle() //handles the point deduction and initial grace period of getting to a target circle
    {
        yield return new WaitForSecondsRealtime(_gettingToCircleBuffer);

        if (!_hasEntered) //need it here because the coroutine operates independently from the initial condition
        {
            while (true)
            {
                _actualScore -= _maxScore/(_gettingToCircleBuffer/_deltaTimeScore);
                yield return new WaitForSecondsRealtime(_deltaTimeScore);
            }
        }
    }

    /// <summary>
    /// This method is used to handle transitions between switching target circles. Only used in the GetNewColourCircle helper class.
    /// </summary>
    public void GetNewCircle()
    {
        //reset all circle score and time values just in case they aren't already these values.
        _actualScore = 250;
        _timeLeftToGetScore = _timeToGetScore;
        _gettingToCircle = StartCoroutine(GettingToCircle());
    }

    /// <summary>
    /// This method is for getting what the player got as a score for completing the circle. Only used to get total score in GameSession.
    /// <summary>
    public float GetScore() => _timeLeftToGetScore ==  _timeToGetScore ? 0 : _actualScore; //conditional operator not totally necessary, but sometimes the timing is a little off and we want to make sure that if the person doesn't go into the target that they get no points
}
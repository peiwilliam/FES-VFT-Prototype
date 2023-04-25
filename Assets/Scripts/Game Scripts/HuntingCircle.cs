using System.Collections;
using UnityEngine;

/// <summary>
/// This class is resposnible for controlling the behaviour of  the hunting targets in the hunting game.
/// </summary>
public class HuntingCircle : MonoBehaviour
{
    [Tooltip("How quickly the score increases and decreases")]
    [SerializeField] private float _deltaTimeScore = 0.25f;
    [Tooltip("How much time is given to the player to go from circle to circle before the maximum score starts to decrease")]
    [SerializeField] private float _gettingToCircleBuffer = 5f;
    [Tooltip("The maximum score per circle")]
    [SerializeField] private int _maxScore = 250;
    [Tooltip("For debugging purposes only, shows when the maximum score is decreasing")]
    [SerializeField] private bool _isDecreasing;
    [Tooltip("For debugging purposes only, shows when the cursor has entered the circle")]
    [SerializeField] private bool _hasEntered;

    private float _actualScore;
    private float _totalDuration;
    private float _timeToGetScore; //How long players need to stay in the circle to get points, stored as ref
    private float _timeLeftToGetScore; //Actually how much time left for the target
    private Color _oldColour; //Just for storing what the original colour is when it switches to green when the player is correct
    private Coroutine _enterCircle; //Coroutine responsible for determining how much time the player has left before they get points
    private Coroutine _exitCircle; //Coroutine responsible for adding to the amount of time needed to stay in the circle if player goes out of the circle
    private Coroutine _gettingToCircle; //Coroutine responsible for giving the players a grace period before they start losing points
    private GameSession _gameSession; //Get the gamesession object for reference values.
    
    private void Awake() //runs only at the beginning when the object is instantiated, needs to be awake so that all necessary variables are instantiated before any coroutines start
    {
        _actualScore = _maxScore;
        _totalDuration = PlayerPrefs.GetInt("Duration of Target", 10);
        _timeToGetScore = PlayerPrefs.GetInt("Duration to Get Points", 3)/2; //the default value used to be 3 but hardcoded to be 1.5 for now
        _timeLeftToGetScore = _timeToGetScore;
        _gameSession = FindObjectOfType<GameSession>();

        if (!_hasEntered) //when the circle spawns, the GettingToCircle coroutine is immediately started
            _gettingToCircle = StartCoroutine(GettingToCircle());
    }
    
    private void OnTriggerEnter2D(Collider2D collider)  //handles what happens when another game obejct collides with the circle
    {
        DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour); //change the colour of the circle to green

        _hasEntered = true; // tells the game that the player has entered at least once.

        if (_gettingToCircle != null) //need to add this account for when cursor is on a target when game starts
            StopCoroutine(_gettingToCircle);

        if (_isDecreasing) //stop the exitcircle coroutine when the player enters the circle
        {
            StopCoroutine(_exitCircle);
            _isDecreasing = false;
        }

        _enterCircle = StartCoroutine(EnterCircle());
    }

    private void OnTriggerExit2D(Collider2D collider) //handles what happens when another game obejct exits the circle
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour); //change back to the old colour
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

        if (_timeLeftToGetScore <= 0)
            _gameSession.ConditionHuntingMet = true;
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

        if (!_hasEntered) //need this condition because the coroutine operates independently from the initial condition
        {
            while (true)
            {
                // _gettingToCircleBuffer is used here instead because if they still haven't reached the circle by this point
                // it'll only be 5 seconds left and we need it to go to zero
                _actualScore -= _maxScore/(_gettingToCircleBuffer/_deltaTimeScore);
                yield return new WaitForSecondsRealtime(_deltaTimeScore);
            }
        }
    }

    /// <summary>
    /// This method is for getting what the player got as a score for completing the circle. Only used to get total score in GameSession.
    /// <summary>
    public float GetScore() => _timeLeftToGetScore ==  _timeToGetScore ? 0 : _actualScore; //conditional operator not totally necessary, but sometimes the timing is a little off and we want to make sure that if the person doesn't go into the target that they get no points
}
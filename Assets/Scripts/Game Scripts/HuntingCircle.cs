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
    [SerializeField] private int _score = 250;
    [Tooltip("For debugging purposes only, shows when the maximum score is decreasing")]
    [SerializeField] private bool _isDecreasing;
    [Tooltip("For debugging purposes only, shows when the cursor has entered the circle")]
    [SerializeField] private bool _hasEntered;

    private float _timeToGetScore; //How long players need to stay in the circle to get points
    private Color _oldColour; //Just for storing what the original colour is when it switches to green when the player is correct
    private Coroutine _enterCircle; //Coroutine responsible for determining how much time the player has left before they get points
    private Coroutine _exitCircle; //Coroutine responsible for adding to the amount of time needed to stay in the circle if player goes out of the circle
    private Coroutine _gettingToCircle; //Coroutine responsible for giving the players a grace period before they start losing points
    private GameSession _gameSession; //Get the gamesession object for reference values.
    
    private void Start() //runs only at the beginning when the object is instantiated
    {
        _timeToGetScore = PlayerPrefs.GetInt("Duration to Get Points", 3); //the default value for the time to get the score is 3
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
        while (_timeToGetScore > 0)
        {
            _timeToGetScore -= Time.unscaledDeltaTime;
            yield return null;
        }

        //this condition is probably unnecessary because the code will never reach here unless the time is less than zero, but just in case
        if (_timeToGetScore <= 0)
        {
            _gameSession.ConditionHuntingMet = true;
            _hasEntered = false;
            _timeToGetScore = 3f;
        }
    }

    private IEnumerator ExitCircle() //handles the addition of time and subtraction of the score when the player goes out of the circle
    {
        _isDecreasing = true;

        while (true)
        {
            _score--;

            if (_timeToGetScore > 0) //add time if not completed time inside circle
                _timeToGetScore += 0.0625f;

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
                _score--;
                yield return new WaitForSecondsRealtime(_deltaTimeScore);
            }
        }
    }

    /// <summary>
    /// This method is for getting what the player got as a score for completing the circle. Only used to get total score in GameSession.
    /// <summary>
    public int GetScore() => _score;
}
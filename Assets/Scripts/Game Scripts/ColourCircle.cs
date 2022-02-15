using System.Collections;
using UnityEngine;

public class ColourCircle : MonoBehaviour
{
    [Tooltip("The time period at which the highest possible score the player can get decreases")]
    [SerializeField] private float _deltaTimeScore = 0.25f;
    [Tooltip("The amount of time allowed to get to a circle before the highest possible score starts to decrease")]
    [SerializeField] private float _gettingToCircleBuffer = 5f;
    [Tooltip("The highest possible score")]
    [SerializeField] private int _score = 250;
    [Tooltip("For debugging purposes only: Shows when the score is decreasing")]
    [SerializeField] private bool _isDecreasing;
    [Tooltip("For debugging purposes only: Shows when the cursor has entered the target circle")]
    [SerializeField] private bool _hasEntered;

    private float _timeToGetScore;
    private Color _oldColour;
    private Coroutine _enterCircle;
    private Coroutine _exitCircle;
    private Coroutine _gettingToCircle;
    private GameSession _gameSession;

    private void Start()
    {
        _timeToGetScore = PlayerPrefs.GetInt("Duration to Get Points", 3);
        _gameSession = FindObjectOfType<GameSession>();
    }

    private void Update() //need this for colour circle since the circles always exist in the game
    {
        if (gameObject.tag == "Untagged") 
        {
            if (_gettingToCircle != null) //_gettingToCircle is always not null, so perfect for resets
                StopAllCoroutines();
            if (gameObject.GetComponent<SpriteRenderer>().color == Color.green)
                DetectCursor.ChangeColourBack(gameObject, _oldColour);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        if (gameObject.GetComponent<ColourCircle>() == _gameSession.TargetColourCircle)
        {
            DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour);

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
    }

    private void OnTriggerExit2D(Collider2D collider) 
    {
        if (_enterCircle != null)
        {
            DetectCursor.ChangeColourBack(gameObject, _oldColour);

            StopCoroutine(_enterCircle);
            _exitCircle = StartCoroutine(ExitCircle());
        }
    }

    private IEnumerator EnterCircle()
    {
        while (_timeToGetScore > 0)
        {
            _timeToGetScore -= Time.unscaledDeltaTime;

            yield return null;
        }

        if (_timeToGetScore <= 0)
        {
            _gameSession.ConditionColourMet = true;
            _hasEntered = false;
            gameObject.tag = "Untagged";
            _timeToGetScore = 3f;
        }
    }

    private IEnumerator ExitCircle()
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

    private IEnumerator GettingToCircle()
    {
        yield return new WaitForSecondsRealtime(_gettingToCircleBuffer);

        if (!_hasEntered) //need it here because the coroutine operates independently from the initial condition
        {
            while (true)
            {
                _score--;
                yield return new WaitForSecondsRealtime(_deltaTimeScore);
            }
        }
    }

    public void GetNewCircle()
    {
        _score = 250;
        _timeToGetScore = 3f;
        _gettingToCircle = StartCoroutine(GettingToCircle());
    }

    public int GetScore() => _score;
}
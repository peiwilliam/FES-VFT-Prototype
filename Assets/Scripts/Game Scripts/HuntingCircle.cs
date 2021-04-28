using System.Collections;
using UnityEngine;

public class HuntingCircle : MonoBehaviour
{
    [SerializeField] private float _deltaTimeScore = 0.25f;
    [SerializeField] private float _gettingToCircleBuffer = 5f;
    [SerializeField] private float _timeToGetScore = 3f;
    [SerializeField] private int _score = 250;
    [SerializeField] private bool _isDecreasing;
    [SerializeField] private bool _hasEntered;

    private Color _oldColour;
    private Coroutine _enterCircle;
    private Coroutine _exitCircle;
    private Coroutine _gettingToCircle;
    private GameSession _gameSession;
    
    private void Start()
    {
        _gameSession = FindObjectOfType<GameSession>();

        if (!_hasEntered)
            _gettingToCircle = StartCoroutine(GettingToCircle());
    }
    
    private void OnTriggerEnter2D(Collider2D collider) 
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

    private void OnTriggerExit2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour);

        StopCoroutine(_enterCircle);
        _exitCircle = StartCoroutine(ExitCircle());
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
            _gameSession.ConditionHuntingMet = true;
            _hasEntered = false;
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

        if (!_hasEntered) //need it here because the coroutine operates independetly from the initial condition
        {
            while (true)
            {
                _score--;
                yield return new WaitForSecondsRealtime(_deltaTimeScore);
            }
        }
    }

    public int GetScore() => _score;
}
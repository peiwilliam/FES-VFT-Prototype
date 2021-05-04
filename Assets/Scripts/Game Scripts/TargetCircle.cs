using System.Collections;
using UnityEngine;

public class TargetCircle : MonoBehaviour
{
    // normally delta time would be in this class, but because circles are interrelated, the variable is created in GameSession
    [SerializeField] private float _score;
    [SerializeField] private float _scoreIncreaseRate = 1f;
    [SerializeField] private float _scoreMultiplier = 1.5f;
    [SerializeField] private float _timeNeededForMultiplier = 5f;
    [SerializeField] private bool _isInCircle;
    [SerializeField] private bool _multiplyScore;
    [SerializeField] private TargetCircle _nextCircle;
    [SerializeField] private GameSession _gameSession;
    
    private Coroutine _increaseScore;
    private Coroutine _multiplier;

    // to access _isInCircle outside of instance
    public bool IsInCircle
    {
        get => _isInCircle;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        _isInCircle = true;
        _increaseScore = StartCoroutine(IncreaseScore());
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        _isInCircle = false;

        if (gameObject.tag == "Target")
        {
            _multiplyScore = false;

            if (_multiplier != null)
            {
                StopCoroutine(_multiplier); //stopping a coroutine doesn't make it null, need to make it null manually
                _multiplier = null;
            }
        }

        StopCoroutine(_increaseScore);
    }

    private void OnTriggerStay2D(Collider2D collider) 
    {
        if (gameObject.tag == "Target" && _multiplier == null)
            _multiplier = StartCoroutine(Multiplier());

        if (_nextCircle != null) //the centre ring doesn't have a next ring, so doesn't need to use this
        {
            if (_nextCircle.IsInCircle && _increaseScore != null) // if the cursor is in the next circle, the outer bigger circles don't contribute points.
            {
                StopCoroutine(_increaseScore); //stopping a coroutine doesn't make it null, need to make it null manually
                _increaseScore = null;
            }
            else if (_increaseScore == null && !_nextCircle.IsInCircle)
                _increaseScore = StartCoroutine(IncreaseScore());
        }
    }

    private IEnumerator IncreaseScore()
    {
        while (true)
        {
            while (_gameSession.DeltaTimeScore > 0)
            {
                _gameSession.DeltaTimeScore -= Time.unscaledDeltaTime;

                yield return null;
            }
            if (_gameSession.DeltaTimeScore <= 0)
                _gameSession.DeltaTimeScore = 0.25f;

            if (_multiplyScore && gameObject.tag == "Target")
                _score += _scoreMultiplier*_scoreIncreaseRate;
            else
                _score += _scoreIncreaseRate;

            //yield return new WaitForSecondsRealtime(_deltaTimeScore);
        }   
    }

    private IEnumerator Multiplier()
    {
        yield return new WaitForSecondsRealtime(_timeNeededForMultiplier); //need to stay inside the circle for a couple seconds to trigger multiplier

        _multiplyScore = true;
    }

    public float GetScore() => _score;
}

using System;
using System.Collections;
using UnityEngine;

public class ColourCircle : MonoBehaviour
{
    [SerializeField] private float _deltaTimeScore = 0.25f;
    [SerializeField] private float _timeToGetScore = 3f;
    [SerializeField] private int _score = 250;
    [SerializeField] private bool _isDecreasing;
    [SerializeField] private bool _hasEntered;

    private Color _oldColour;
    private Coroutine _enterCircle;
    private Coroutine _exitCircle;
    private Coroutine _gettingToCircle;
    private ColourCircle _currentCircle;
    private GameSession _gameSession;

    private void Start()
    {
        _gameSession = FindObjectOfType<GameSession>();

        if (!_hasEntered)
            _gettingToCircle = StartCoroutine(GettingToCircle());
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        _currentCircle = _gameSession.TargetColourCircle;

        if (gameObject.GetComponent<ColourCircle>() == _currentCircle)
        {
            DetectCursor.ChangeColourOnDetection(gameObject, out _oldColour);

            _hasEntered = true; // tells the game that the player has entered at least once.

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
            _timeToGetScore -= Time.deltaTime;

            yield return null;
        }

        if (_timeToGetScore <= 0)
        {
            _gameSession.ConditionMet = true;
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

            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    private IEnumerator GettingToCircle()
    {
        yield return new WaitForSeconds(5f);

        if (!_hasEntered)
        {
            while (true)
            {
                _score--;
                yield return new WaitForSeconds(_deltaTimeScore);
            }
        }
    }

    public int GetScore() => _score;

    // public delegate void CompletedCircleEventHandler(object source, EventArgs eventArgs);
    // public event CompletedCircleEventHandler CompletedCircle;

    // protected virtual void OnCompletedCircle()
    // {
    //     if (CompletedCircle != null)
    //         CompletedCircle(this, EventArgs.Empty);
    // }
}
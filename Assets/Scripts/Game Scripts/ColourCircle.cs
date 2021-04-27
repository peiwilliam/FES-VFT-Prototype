﻿using System.Collections;
using UnityEngine;

public class ColourCircle : MonoBehaviour
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
    }

    private void Update() //need this for colour circle since the circles always exist in the game
    {
        if (!_hasEntered && gameObject.tag == "Target")
        {
            //these need to be reset as it's a new target
            gameObject.GetComponent<ColourCircle>()._score = 250;
            gameObject.GetComponent<ColourCircle>()._timeToGetScore = 3f;

            _gettingToCircle = StartCoroutine(GettingToCircle());
        }
        else if (gameObject.tag == "Untagged" && _gettingToCircle != null) //_gettingToCircle is always not null, so perfect for resets
        {
            if (_gettingToCircle != null)
                StopAllCoroutines();
            if (gameObject.GetComponent<SpriteRenderer>().color == new Color(0, 255, 0))
                DetectCursor.ChangeColourBack(gameObject, _oldColour);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider) 
    {
        if (gameObject.GetComponent<ColourCircle>() == _gameSession.TargetColourCircle)
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

        if (!_hasEntered)
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
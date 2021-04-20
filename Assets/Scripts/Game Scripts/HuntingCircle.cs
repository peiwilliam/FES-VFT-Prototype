using System.Collections;
using UnityEngine;

public class HuntingCircle : MonoBehaviour
{
    [SerializeField] private float _deltaTimeScore = 0.2f;
    [SerializeField] private int _score;
    [SerializeField] private bool _isDecreasing;
    [SerializeField] private bool _hasEntered;

    private Color _oldColour;
    private Coroutine _enterCircle;
    private Coroutine _exitCircle;
    private Coroutine _gettingToCircle;
    
    private void Start()
    {
        if (!_hasEntered)
            _gettingToCircle = StartCoroutine(GettingToCircle());
    }
    
    private void OnTriggerEnter2D(Collider2D collider) 
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

    private void OnTriggerExit2D(Collider2D collider) 
    {
        DetectCursor.ChangeColourBack(gameObject, _oldColour);

        StopCoroutine(_enterCircle);
        _exitCircle = StartCoroutine(ExitCircle());
    }

    private IEnumerator EnterCircle()
    {
        while (true)
        {
            _score++;
            yield return new WaitForSeconds(_deltaTimeScore);
        }
    }

    private IEnumerator ExitCircle()
    {
        _isDecreasing = true;

        while (true)
        {
            _score--;
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
}

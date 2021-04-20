using System.Collections;
using UnityEngine;

public class TargetCircle : MonoBehaviour
{
    [SerializeField] private int _score;
    [SerializeField] private int _scoreIncreaseRate = 1; //default is 1, can be changed
    
    private Coroutine _increaseScore;
    
    private void OnTriggerEnter2D(Collider2D collider) => _increaseScore = StartCoroutine(IncreaseScore());

    private void OnTriggerExit2D(Collider2D collider) => StopCoroutine(_increaseScore);

    private IEnumerator IncreaseScore()
    {
        while (true)
        {
            _score += _scoreIncreaseRate;
            yield return new WaitForSeconds(0.2f);
        }   
    }

    public int GetScore() => _score;
}

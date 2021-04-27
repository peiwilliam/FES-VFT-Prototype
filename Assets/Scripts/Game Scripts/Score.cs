using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Score : MonoBehaviour
{
    [SerializeField] private GameSession _gameSession;

    private void Update()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Colour Matching":
                gameObject.GetComponent<Text>().text = _gameSession.ColourMatchingScore.ToString();
                break;
            case "Ellipse":
                gameObject.GetComponent<Text>().text = _gameSession.EllipseScore.ToString();
                break;
            case "Hunting":
                gameObject.GetComponent<Text>().text = _gameSession.HuntingScore.ToString();
                break;
            case "Target":
                gameObject.GetComponent<Text>().text = _gameSession.TargetScore.ToString();
                break;
        }
    }
}

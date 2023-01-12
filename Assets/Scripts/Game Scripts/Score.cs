using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for keeping track of the score in the games and displaying it.
/// </summary>
public class Score : MonoBehaviour
{
    [Tooltip("GameSession object for the game")]
    [SerializeField] private GameSession _gameSession;

    private void Update() //runs at every frame update
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

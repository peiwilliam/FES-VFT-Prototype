using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    [SerializeField] private GameObject _cursorPrefab;
    [SerializeField] private MovingCircle _targetPrefab;

    private Ellipse _ellipse;

    // private void Awake() 
    // {
    //     SetUpSingleton(); 
    // }

    // private void SetUpSingleton()
    // {
    //     var numberOfSessions = FindObjectsOfType<GameSession>().Length;

    //     if (numberOfSessions > 1)
    //         Destroy(gameObject);
    //     else
    //         DontDestroyOnLoad(gameObject);
    // }
    
    // Start is called before the first frame update
    private void Start()
    {
        Instantiate(_cursorPrefab, new Vector3(0, 0, 0), Quaternion.identity); //need cursor for all games

        switch (SceneManager.GetActiveScene().name)
        {
            case "Colour Matching":
                break;
            case "Ellipse":
                EllipseGame();
                break;
            case "Target":
                break;
            case "Hunting":
                break;
        }
    }

    private void EllipseGame()
    {
        _ellipse = FindObjectOfType<Ellipse>();
        var radii = _ellipse.GetRadii();
        var centre = _ellipse.GetCentre();
        Instantiate(_targetPrefab, new Vector3(radii[0] + centre[0], centre[1], 0), Quaternion.identity);
    }
}

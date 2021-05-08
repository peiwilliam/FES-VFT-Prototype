using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private ZeroBoard _zeroBoard;

    private static bool _beginFamiliarization;
    private static int _gameIndex = 1; //starting value
    private static bool _beginExperimentation;
    private static List<int> _gameIndices = new List<int> {2, 3, 4, 5}; //starting values +1 from 1,2,3,4 because of build index
    private static int _gameIndicesIndex;

    public void LoadNextScene()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    public void LoadNextGame()
    {
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (SceneManager.GetActiveScene().name == "Experimentation Transition")
        {
            _gameIndex = _gameIndices[++_gameIndicesIndex];
            SceneManager.LoadScene(currentSceneIndex + _gameIndex);
        }
        else
            SceneManager.LoadScene(currentSceneIndex + _gameIndex);
    }

    public void LoadStartScene() => SceneManager.LoadScene(0);

    public void ConnectToBoard() => SceneManager.LoadScene("Bluetooth PIN");

    public void LoadSettings() => SceneManager.LoadScene("Settings");

    public void ZeroBoard() => Instantiate(_zeroBoard);

    public void LoadColourMatching() => SceneManager.LoadScene("Colour Matching");

    public void LoadEllipse() => SceneManager.LoadScene("Ellipse");

    public void LoadTarget() => SceneManager.LoadScene("Target");

    public void LoadHunting() => SceneManager.LoadScene("Hunting");

    public void QuitGame() => Application.Quit();

    public void BeginAssesment() => SceneManager.LoadScene("Assessment"); 

    public void Familiarization()
    {
        if (!_beginFamiliarization)
            _beginFamiliarization = true;
        else if (_gameIndex < 4)
            _gameIndex++;

        SceneManager.LoadScene("Familiarization Transition");
    }

    public void Experimentation() 
    {
        if (!_beginExperimentation)
            _beginExperimentation = true;
        else if (_gameIndices == new List<int> {1, 2, 3, 4})
        {
            
        }

        _gameIndex = _gameIndices[_gameIndicesIndex++]; //zero first then increment

        SceneManager.LoadScene("Experimentation Transition");
    }

    public static bool GetFamiliarization() => _beginFamiliarization;

    public static int GetGameIndex() => _gameIndex;

    public static bool GetExperimentation() => _beginExperimentation;

    private void OnEnable() 
    {
        if (SceneManager.GetActiveScene().name == "Settings")
        {
            var settingsManager = FindObjectOfType<SettingsManager>();
            settingsManager.SetInputFields();
        }
    }
}